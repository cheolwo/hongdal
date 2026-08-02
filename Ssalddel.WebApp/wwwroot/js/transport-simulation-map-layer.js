const DEFAULT_RENDERER_KIND = "google-overlay-canvas";
const rendererFactories = new Map();

export function registerTransportSimulationRenderer(kind, factory) {
    if (!kind || typeof factory !== "function") {
        throw new TypeError("A renderer kind and factory are required.");
    }

    rendererFactories.set(kind, factory);
}

export function createTransportSimulationLayer(map, scenarios, options = {}) {
    const rendererKind = options.rendererKind ?? DEFAULT_RENDERER_KIND;
    const factory = rendererFactories.get(rendererKind);
    if (!factory) {
        throw new Error(`Unknown transport simulation renderer: ${rendererKind}`);
    }

    return factory(map, scenarios, options);
}

registerTransportSimulationRenderer(
    DEFAULT_RENDERER_KIND,
    (map, scenarios, options) => new GoogleOverlayCanvasSimulationRenderer(map, scenarios, options));

class GoogleOverlayCanvasSimulationRenderer extends google.maps.OverlayView {
    constructor(map, scenarios, options) {
        super();
        this.map = map;
        this.canvas = undefined;
        this.context = undefined;
        this.frameRequest = undefined;
        this.lastFrameAt = 0;
        this.pixelRatio = 1;
        this.scenarios = [];
        this.options = {};
        this.update(scenarios, options);
        this.setMap(map);
    }

    onAdd() {
        const canvas = document.createElement("canvas");
        canvas.setAttribute("aria-hidden", "true");
        canvas.dataset.renderer = DEFAULT_RENDERER_KIND;
        canvas.style.position = "absolute";
        canvas.style.inset = "0";
        canvas.style.pointerEvents = "none";
        canvas.style.zIndex = "2";
        this.canvas = canvas;
        this.context = canvas.getContext("2d");
        this.getPanes()?.overlayLayer.appendChild(canvas);
    }

    draw() {
        this.resizeCanvas();
        this.render(performance.now());
        this.scheduleAnimationFrame();
    }

    onRemove() {
        this.cancelAnimationFrame();
        this.canvas?.remove();
        this.canvas = undefined;
        this.context = undefined;
    }

    update(scenarios, options = {}) {
        this.scenarios = Array.isArray(scenarios)
            ? scenarios.filter(isSafeSimulatedScenario)
            : [];
        this.options = {
            animationEnabled: options.animationEnabled !== false,
            maxVisibleObjects: clampNumber(options.maxVisibleObjects, 1, 24, 12),
            rendererKind: options.rendererKind ?? DEFAULT_RENDERER_KIND
        };
        this.cancelAnimationFrame();
        if (this.canvas) {
            this.draw();
        }
    }

    dispose() {
        this.setMap(null);
    }

    resizeCanvas() {
        if (!this.canvas || !this.context) {
            return;
        }

        const mapElement = this.map.getDiv();
        const width = Math.max(1, mapElement.clientWidth);
        const height = Math.max(1, mapElement.clientHeight);
        this.pixelRatio = Math.min(globalThis.devicePixelRatio || 1, 2);
        const renderWidth = Math.round(width * this.pixelRatio);
        const renderHeight = Math.round(height * this.pixelRatio);
        if (this.canvas.width !== renderWidth || this.canvas.height !== renderHeight) {
            this.canvas.width = renderWidth;
            this.canvas.height = renderHeight;
            this.canvas.style.width = `${width}px`;
            this.canvas.style.height = `${height}px`;
        }
    }

    render(now) {
        const projection = this.getProjection();
        if (!projection || !this.canvas || !this.context) {
            return;
        }

        const context = this.context;
        const width = this.canvas.width / this.pixelRatio;
        const height = this.canvas.height / this.pixelRatio;
        context.setTransform(this.pixelRatio, 0, 0, this.pixelRatio, 0, 0);
        context.clearRect(0, 0, width, height);

        const visibleScenarios = this.visibleScenarios();
        for (const scenario of visibleScenarios) {
            const points = scenario.route
                .map(point => projection.fromLatLngToDivPixel(
                    new google.maps.LatLng(point.latitude, point.longitude)))
                .filter(Boolean);
            if (points.length < 2) {
                continue;
            }

            drawRoute(context, points, scenario.color);
            const progress = this.animationIsEnabled()
                ? (now / (Math.max(6, scenario.animationCycleSeconds) * 1000)) % 1
                : 0.42;
            const location = pointAlongPolyline(points, progress);
            drawVehicle(context, scenario.modeCode, location, scenario.color);
        }
    }

    visibleScenarios() {
        const bounds = this.map.getBounds();
        const zoom = this.map.getZoom() ?? 2;
        const limit = visibleObjectLimitForZoom(zoom, this.options.maxVisibleObjects);
        return this.scenarios
            .filter(scenario => routeTouchesViewport(scenario.route, bounds, zoom))
            .slice(0, limit);
    }

    animationIsEnabled() {
        return this.options.animationEnabled
            && !globalThis.matchMedia?.("(prefers-reduced-motion: reduce)")?.matches;
    }

    scheduleAnimationFrame() {
        if (!this.animationIsEnabled() || this.frameRequest !== undefined) {
            return;
        }

        const tick = now => {
            this.frameRequest = undefined;
            if (now - this.lastFrameAt >= 32) {
                this.lastFrameAt = now;
                this.render(now);
            }
            this.scheduleAnimationFrame();
        };
        this.frameRequest = globalThis.requestAnimationFrame(tick);
    }

    cancelAnimationFrame() {
        if (this.frameRequest === undefined) {
            return;
        }

        globalThis.cancelAnimationFrame(this.frameRequest);
        this.frameRequest = undefined;
    }
}

function isSafeSimulatedScenario(scenario) {
    return scenario?.isSimulation === true
        && scenario?.sourceKindCode === "simulated-fixture"
        && typeof scenario?.simulationMark === "string"
        && scenario.simulationMark.includes("SIMULATED")
        && Array.isArray(scenario.route)
        && scenario.route.length >= 2;
}

function visibleObjectLimitForZoom(zoom, configuredMaximum) {
    if (zoom <= 3) {
        return Math.min(configuredMaximum, 3);
    }
    if (zoom <= 5) {
        return Math.min(configuredMaximum, 6);
    }
    return configuredMaximum;
}

function routeTouchesViewport(route, bounds, zoom) {
    if (!bounds || zoom <= 3) {
        return true;
    }

    return route.some(point => bounds.contains(
        new google.maps.LatLng(point.latitude, point.longitude)));
}

function drawRoute(context, points, color) {
    context.save();
    context.beginPath();
    context.moveTo(points[0].x, points[0].y);
    for (let index = 1; index < points.length; index += 1) {
        context.lineTo(points[index].x, points[index].y);
    }
    context.strokeStyle = color || "#2f6fab";
    context.globalAlpha = 0.76;
    context.lineWidth = 2.5;
    context.setLineDash([8, 7]);
    context.stroke();
    context.restore();
}

function drawVehicle(context, modeCode, location, color) {
    context.save();
    context.translate(location.x, location.y);
    context.rotate(location.angle);
    context.fillStyle = color || "#2f6fab";
    context.strokeStyle = "#ffffff";
    context.lineWidth = 2;
    context.shadowColor = "rgba(15, 23, 42, .28)";
    context.shadowBlur = 6;

    if (modeCode === "aviation") {
        context.beginPath();
        context.moveTo(13, 0);
        context.lineTo(-7, -6);
        context.lineTo(-3, 0);
        context.lineTo(-7, 6);
        context.closePath();
    } else if (modeCode === "maritime") {
        context.beginPath();
        context.moveTo(11, 0);
        context.lineTo(5, 7);
        context.lineTo(-10, 5);
        context.lineTo(-12, -5);
        context.lineTo(5, -7);
        context.closePath();
    } else {
        context.beginPath();
        context.roundRect(-11, -7, 16, 12, 3);
        context.rect(5, -4, 7, 9);
    }
    context.fill();
    context.stroke();
    context.restore();
}

function pointAlongPolyline(points, progress) {
    const segments = [];
    let totalLength = 0;
    for (let index = 1; index < points.length; index += 1) {
        const start = points[index - 1];
        const end = points[index];
        const length = Math.hypot(end.x - start.x, end.y - start.y);
        segments.push({ start, end, length });
        totalLength += length;
    }

    let target = totalLength * Math.max(0, Math.min(progress, 1));
    for (const segment of segments) {
        if (target <= segment.length || segment === segments.at(-1)) {
            const ratio = segment.length === 0 ? 0 : target / segment.length;
            return {
                x: segment.start.x + ((segment.end.x - segment.start.x) * ratio),
                y: segment.start.y + ((segment.end.y - segment.start.y) * ratio),
                angle: Math.atan2(
                    segment.end.y - segment.start.y,
                    segment.end.x - segment.start.x)
            };
        }
        target -= segment.length;
    }

    return { x: points[0].x, y: points[0].y, angle: 0 };
}

function clampNumber(value, minimum, maximum, fallback) {
    const numericValue = Number(value);
    if (!Number.isFinite(numericValue)) {
        return fallback;
    }
    return Math.min(maximum, Math.max(minimum, numericValue));
}
