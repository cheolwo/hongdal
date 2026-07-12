export function toDiagramPoint(element, clientX, clientY) {
  if (!element) {
    return { x: 0, y: 0 };
  }

  const rect = element.getBoundingClientRect();
  const width = rect.width || 1;
  const height = rect.height || 1;
  const x = clamp(((clientX - rect.left) / width) * 100, 0, 100);
  const y = clamp(clientY - rect.top, 0, height);
  return { x, y };
}

export function findConnectionHandle(clientX, clientY) {
  const element = document
    .elementFromPoint(clientX, clientY)
    ?.closest(".platform-ledger-flow-handle");

  const resolved = element || findNearestConnectionHandle(clientX, clientY);
  if (!resolved) {
    return null;
  }

  return {
    nodeTitle: resolved.dataset.nodeTitle || "",
    handle: resolved.dataset.handle || ""
  };
}

function findNearestConnectionHandle(clientX, clientY) {
  let nearest = null;
  let nearestDistance = Number.POSITIVE_INFINITY;
  for (const handle of document.querySelectorAll(".platform-ledger-flow-handle")) {
    const rect = handle.getBoundingClientRect();
    const x = rect.left + rect.width / 2;
    const y = rect.top + rect.height / 2;
    const distance = Math.hypot(clientX - x, clientY - y);
    if (distance < nearestDistance) {
      nearest = handle;
      nearestDistance = distance;
    }
  }

  return nearestDistance <= 24 ? nearest : null;
}

function clamp(value, min, max) {
  return Math.max(min, Math.min(max, value));
}
