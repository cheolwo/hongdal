const pads = new Map();

export function initialize(canvasId, options) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        return;
    }

    dispose(canvasId);

    const context = canvas.getContext("2d");
    const state = {
        canvas,
        context,
        isDrawing: false,
        isEmpty: true,
        strokeCount: 0,
        disabled: Boolean(options?.disabled),
        strokeColor: options?.strokeColor || "#111827",
        strokeWidth: Number(options?.strokeWidth || 2.4),
        handlers: {}
    };

    resizeCanvas(state);

    state.handlers.pointerDown = event => startStroke(state, event);
    state.handlers.pointerMove = event => moveStroke(state, event);
    state.handlers.pointerUp = event => endStroke(state, event);
    state.handlers.pointerLeave = event => endStroke(state, event);
    state.handlers.resize = () => resizeCanvas(state, true);

    canvas.addEventListener("pointerdown", state.handlers.pointerDown);
    canvas.addEventListener("pointermove", state.handlers.pointerMove);
    canvas.addEventListener("pointerup", state.handlers.pointerUp);
    canvas.addEventListener("pointercancel", state.handlers.pointerUp);
    canvas.addEventListener("pointerleave", state.handlers.pointerLeave);
    window.addEventListener("resize", state.handlers.resize);

    pads.set(canvasId, state);
}

export function setDisabled(canvasId, disabled) {
    const state = pads.get(canvasId);
    if (!state) {
        return;
    }

    state.disabled = Boolean(disabled);
}

export function clear(canvasId) {
    const state = pads.get(canvasId);
    if (!state) {
        return;
    }

    state.context.clearRect(0, 0, state.canvas.width, state.canvas.height);
    state.isEmpty = true;
    state.strokeCount = 0;
}

export function capture(canvasId) {
    const state = pads.get(canvasId);
    if (!state) {
        return {
            isEmpty: true,
            dataUrl: "",
            strokeCount: 0,
            width: 0,
            height: 0
        };
    }

    return {
        isEmpty: state.isEmpty,
        dataUrl: state.isEmpty ? "" : state.canvas.toDataURL("image/png"),
        strokeCount: state.strokeCount,
        width: state.canvas.width,
        height: state.canvas.height
    };
}

export function dispose(canvasId) {
    const state = pads.get(canvasId);
    if (!state) {
        return;
    }

    state.canvas.removeEventListener("pointerdown", state.handlers.pointerDown);
    state.canvas.removeEventListener("pointermove", state.handlers.pointerMove);
    state.canvas.removeEventListener("pointerup", state.handlers.pointerUp);
    state.canvas.removeEventListener("pointercancel", state.handlers.pointerUp);
    state.canvas.removeEventListener("pointerleave", state.handlers.pointerLeave);
    window.removeEventListener("resize", state.handlers.resize);
    pads.delete(canvasId);
}

function resizeCanvas(state, preserveImage = false) {
    const { canvas, context } = state;
    const previousImage = preserveImage && !state.isEmpty
        ? canvas.toDataURL("image/png")
        : null;
    const rect = canvas.getBoundingClientRect();
    const ratio = Math.max(window.devicePixelRatio || 1, 1);
    const width = Math.max(Math.floor(rect.width * ratio), 1);
    const height = Math.max(Math.floor(rect.height * ratio), 1);

    if (canvas.width === width && canvas.height === height) {
        return;
    }

    canvas.width = width;
    canvas.height = height;
    context.setTransform(1, 0, 0, 1, 0, 0);
    context.scale(ratio, ratio);
    context.lineCap = "round";
    context.lineJoin = "round";

    if (!previousImage) {
        return;
    }

    const image = new Image();
    image.onload = () => {
        context.drawImage(image, 0, 0, rect.width, rect.height);
    };
    image.src = previousImage;
}

function startStroke(state, event) {
    if (state.disabled) {
        return;
    }

    event.preventDefault();
    state.canvas.setPointerCapture?.(event.pointerId);
    state.isDrawing = true;
    state.strokeCount += 1;
    const point = getPoint(state.canvas, event);
    state.context.beginPath();
    state.context.moveTo(point.x, point.y);
}

function moveStroke(state, event) {
    if (!state.isDrawing || state.disabled) {
        return;
    }

    event.preventDefault();
    const point = getPoint(state.canvas, event);
    state.context.strokeStyle = state.strokeColor;
    state.context.lineWidth = state.strokeWidth;
    state.context.lineTo(point.x, point.y);
    state.context.stroke();
    state.isEmpty = false;
}

function endStroke(state, event) {
    if (!state.isDrawing) {
        return;
    }

    event.preventDefault();
    state.isDrawing = false;
    state.canvas.releasePointerCapture?.(event.pointerId);
}

function getPoint(canvas, event) {
    const rect = canvas.getBoundingClientRect();
    return {
        x: event.clientX - rect.left,
        y: event.clientY - rect.top
    };
}
