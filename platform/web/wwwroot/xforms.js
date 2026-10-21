// XForms on the web: the DOM side of the platform. .NET creates and
// arranges elements by id, sends each layer's drawing as one command list,
// and gets pointer, frame and theme events back.

const elements = new Map();
const images = new Map();
let nextId = 1;
let exports = null;
let root = null;

// Pointers pressed on an element that took them, by pointer id
const pointerOwners = new Map();

const measureCanvas = document.createElement('canvas');
const measureContext = measureCanvas.getContext('2d');
const metricsCache = new Map();

export async function init() {
    const runtime = await globalThis.getDotnetRuntime(0);
    exports = (await runtime.getAssemblyExports('XForms.Framework.Web')).XForms.Web.Js;

    document.documentElement.style.height = '100%';
    document.body.style.margin = '0';
    document.body.style.height = '100%';
    document.body.style.overflow = 'hidden';

    root = document.createElement('div');
    root.style.position = 'fixed';
    root.style.inset = '0';
    root.style.overflow = 'hidden';
    root.style.touchAction = 'none';
    root.style.userSelect = 'none';
    root.style.webkitUserSelect = 'none';
    document.body.appendChild(root);

    window.addEventListener('resize', () => exports.OnResize());
    for (const query of ['(prefers-color-scheme: dark)', '(prefers-contrast: more)', '(forced-colors: active)']) {
        window.matchMedia(query).addEventListener('change', () => exports.OnThemeChanged());
    }

    root.addEventListener('pointerdown', onPointerDown);
    root.addEventListener('pointermove', onPointerMove);
    root.addEventListener('pointerup', (e) => onPointerEnd(e, 3));
    root.addEventListener('pointercancel', (e) => onPointerEnd(e, 4));
    root.addEventListener('pointerleave', (e) => {
        if (e.pointerType !== 'touch') {
            exports.OnPointerLeave(e.pointerId);
        }
    });
    root.addEventListener('wheel', onWheel, { passive: false });
}

// Elements

export function create(kind) {
    const element = document.createElement(kind);
    const id = nextId++;
    element.dataset.xid = id;
    element.id = 'x' + id;
    element.style.position = 'absolute';
    element.style.left = '0';
    element.style.top = '0';
    element.style.boxSizing = 'border-box';
    element.style.margin = '0';
    elements.set(id, element);
    return id;
}

export function destroy(id) {
    const element = elements.get(id);
    if (element) {
        element.remove();
        elements.delete(id);
    }
}

export function setRoot(id) {
    root.replaceChildren(elements.get(id));
}

// Inserts the child before the parent's child at the index (or last, for -1)
export function insert(parentId, childId, index) {
    const parent = elements.get(parentId);
    const child = elements.get(childId);
    const before = (index >= 0) ? (parent.children[index] ?? null) : null;
    if (before !== child) {
        parent.insertBefore(child, before);
    }
}

export function insertBefore(parentId, childId, beforeId) {
    const parent = elements.get(parentId);
    const child = elements.get(childId);
    const before = beforeId ? elements.get(beforeId) : null;
    if (child.parentElement !== parent || child.nextSibling !== before) {
        parent.insertBefore(child, before);
    }
}

// Puts the accessible elements (positive ids) among the native ones
// (negative ids), in the given order, moving only the accessible ones
export function placeAccessible(parentId, ids) {
    const parent = elements.get(parentId);
    let previous = (parent.firstChild?.tagName === 'CANVAS') ? parent.firstChild : null;
    for (const id of ids) {
        const element = elements.get(Math.abs(id));
        if (id > 0) {
            const before = previous ? previous.nextSibling : parent.firstChild;
            if (before !== element) {
                parent.insertBefore(element, before);
            }
        }
        if (element.parentElement === parent) {
            previous = element;
        }
    }
}

export function detach(id) {
    elements.get(id)?.remove();
}

export function setBounds(id, x, y, width, height) {
    const style = elements.get(id).style;
    style.left = x + 'px';
    style.top = y + 'px';
    style.width = width + 'px';
    style.height = height + 'px';
}

export function setStyle(id, name, value) {
    const style = elements.get(id).style;
    if (value === null || value === '') {
        style.removeProperty(name);
    } else {
        style.setProperty(name, value);
    }
}

export function setAttribute(id, name, value) {
    const element = elements.get(id);
    if (value === null) {
        element.removeAttribute(name);
    } else {
        element.setAttribute(name, value);
    }
}

export function setText(id, text) {
    elements.get(id).textContent = text;
}

// Styled runs of text: one span per run, each with its CSS
export function setRuns(id, texts, styles) {
    const element = elements.get(id);
    element.replaceChildren();
    for (let i = 0; i < texts.length; i++) {
        const span = document.createElement('span');
        span.textContent = texts[i];
        span.style.cssText = styles[i];
        element.appendChild(span);
    }
}

export function setTitle(title) {
    document.title = title;
}

// Text

export function measureText(font, text) {
    measureContext.font = font;
    return measureContext.measureText(text).width;
}

// The font's ascent and descent, in CSS pixels
export function fontMetrics(font) {
    let metrics = metricsCache.get(font);
    if (!metrics) {
        measureContext.font = font;
        const m = measureContext.measureText('Hg');
        metrics = [m.fontBoundingBoxAscent, m.fontBoundingBoxDescent];
        metricsCache.set(font, metrics);
    }
    return metrics;
}

// The viewport and the device pixel ratio

export function viewport() {
    return [window.innerWidth, window.innerHeight, window.devicePixelRatio || 1];
}

// Scheduling

export function requestFrame() {
    requestAnimationFrame((time) => exports.OnFrame(time));
}

export function post() {
    setTimeout(() => exports.RunPosted(), 0);
}

// Theme

// Dark (1), more contrast (2), forced colors (4)
export function themeState() {
    return (window.matchMedia('(prefers-color-scheme: dark)').matches ? 1 : 0)
        | (window.matchMedia('(prefers-contrast: more)').matches ? 2 : 0)
        | (window.matchMedia('(forced-colors: active)').matches ? 4 : 0);
}

// Images

export function loadImage(imageId, url) {
    const image = new Image();
    image.onload = () => {
        images.set(imageId, image);
        exports.OnImageLoaded(imageId, image.naturalWidth, image.naturalHeight);
    };
    image.onerror = () => exports.OnImageLoaded(imageId, 0, 0);
    image.src = url;
}

// Shows an image in an <img> element; reports its natural size when loaded
export function setImageSource(id, url, imageId) {
    const element = elements.get(id);
    element.onload = () => exports.OnImageLoaded(imageId, element.naturalWidth, element.naturalHeight);
    element.onerror = () => exports.OnImageLoaded(imageId, 0, 0);
    element.src = url;
}

// Drawing: replays a layer's command list on its canvas

function color(packed) {
    const a = Math.floor(packed / 16777216) & 255;
    const r = Math.floor(packed / 65536) & 255;
    const g = Math.floor(packed / 256) & 255;
    const b = packed & 255;
    return `rgba(${r},${g},${b},${a / 255})`;
}

const tintCache = new Map();

function tinted(image, packed, width, height) {
    const key = `${image.src}|${packed}|${width}|${height}`;
    let canvas = tintCache.get(key);
    if (!canvas) {
        canvas = document.createElement('canvas');
        canvas.width = Math.max(1, Math.ceil(width));
        canvas.height = Math.max(1, Math.ceil(height));
        const context = canvas.getContext('2d');
        context.drawImage(image, 0, 0, canvas.width, canvas.height);
        context.globalCompositeOperation = 'source-in';
        context.fillStyle = color(packed);
        context.fillRect(0, 0, canvas.width, canvas.height);
        if (tintCache.size > 200) {
            tintCache.clear();
        }
        tintCache.set(key, canvas);
    }
    return canvas;
}

function roundRect(context, x, y, w, h, r) {
    context.beginPath();
    context.roundRect(x, y, w, h, Math.max(0, Math.min(r, w / 2, h / 2)));
}

function ellipse(context, cx, cy, rx, ry) {
    context.beginPath();
    context.ellipse(cx, cy, Math.max(0, rx), Math.max(0, ry), 0, 0, Math.PI * 2);
}

export function draw(canvasId, width, height, n, s) {
    const canvas = elements.get(canvasId);
    if (!canvas) {
        return;
    }

    const ratio = window.devicePixelRatio || 1;
    const pixelWidth = Math.max(1, Math.ceil(width * ratio));
    const pixelHeight = Math.max(1, Math.ceil(height * ratio));
    if (canvas.width !== pixelWidth || canvas.height !== pixelHeight) {
        canvas.width = pixelWidth;
        canvas.height = pixelHeight;
    }
    canvas.style.width = width + 'px';
    canvas.style.height = height + 'px';

    const c = canvas.getContext('2d');
    c.setTransform(ratio, 0, 0, ratio, 0, 0);
    c.clearRect(0, 0, width, height);
    c.lineCap = 'butt';
    c.textBaseline = 'alphabetic';

    let i = 0;
    let gradient = null;
    const fillStyle = (packed) => gradient ?? color(packed);

    while (i < n.length) {
        const op = n[i++];
        switch (op) {
            case 1: c.save(); break;
            case 2: c.restore(); break;
            case 3: c.beginPath(); c.rect(n[i], n[i + 1], n[i + 2], n[i + 3]); c.clip(); i += 4; break;
            case 4: c.transform(n[i], n[i + 1], n[i + 2], n[i + 3], n[i + 4], n[i + 5]); i += 6; break;
            case 5: c.globalAlpha *= n[i++]; break;
            case 10: c.fillStyle = fillStyle(n[i + 4]); c.fillRect(n[i], n[i + 1], n[i + 2], n[i + 3]); gradient = null; i += 5; break;
            case 11: c.strokeStyle = color(n[i + 4]); c.lineWidth = n[i + 5]; c.strokeRect(n[i], n[i + 1], n[i + 2], n[i + 3]); i += 6; break;
            case 12: roundRect(c, n[i], n[i + 1], n[i + 2], n[i + 3], n[i + 4]); c.fillStyle = fillStyle(n[i + 5]); c.fill(); gradient = null; i += 6; break;
            case 13: roundRect(c, n[i], n[i + 1], n[i + 2], n[i + 3], n[i + 4]); c.strokeStyle = color(n[i + 5]); c.lineWidth = n[i + 6]; c.stroke(); i += 7; break;
            case 14: ellipse(c, n[i], n[i + 1], n[i + 2], n[i + 3]); c.fillStyle = fillStyle(n[i + 4]); c.fill(); gradient = null; i += 5; break;
            case 15: ellipse(c, n[i], n[i + 1], n[i + 2], n[i + 3]); c.strokeStyle = color(n[i + 4]); c.lineWidth = n[i + 5]; c.stroke(); i += 6; break;
            case 16:
                c.beginPath(); c.moveTo(n[i], n[i + 1]); c.lineTo(n[i + 2], n[i + 3]);
                c.strokeStyle = color(n[i + 4]); c.lineWidth = n[i + 5]; c.stroke(); i += 6; break;
            case 17: {
                // An arc clockwise from the start angle, with 0 at the top
                const start = (n[i + 3] - 90) * Math.PI / 180;
                const end = start + (n[i + 4] * Math.PI / 180);
                c.beginPath(); c.arc(n[i], n[i + 1], Math.max(0, n[i + 2]), start, end, n[i + 4] < 0);
                c.strokeStyle = color(n[i + 5]); c.lineWidth = n[i + 6]; c.lineCap = 'round'; c.stroke(); c.lineCap = 'butt';
                i += 7; break;
            }
            case 18:
                c.font = s[n[i + 3]]; c.fillStyle = color(n[i + 4]);
                c.fillText(s[n[i + 2]], n[i], n[i + 1]); i += 5; break;
            case 19: {
                const image = images.get(n[i]);
                if (image) {
                    const packed = n[i + 5];
                    const source = packed ? tinted(image, packed, n[i + 3] * ratio, n[i + 4] * ratio) : image;
                    c.drawImage(source, n[i + 1], n[i + 2], n[i + 3], n[i + 4]);
                }
                i += 6; break;
            }
            case 20: {
                // A path: the segment count, then each segment's kind and points;
                // then fill (0, color) or stroke (1, color, width)
                const count = n[i++];
                c.beginPath();
                for (let k = 0; k < count; k++) {
                    const kind = n[i++];
                    switch (kind) {
                        case 0: c.moveTo(n[i], n[i + 1]); i += 2; break;
                        case 1: c.lineTo(n[i], n[i + 1]); i += 2; break;
                        case 2: c.quadraticCurveTo(n[i], n[i + 1], n[i + 2], n[i + 3]); i += 4; break;
                        case 3: c.bezierCurveTo(n[i], n[i + 1], n[i + 2], n[i + 3], n[i + 4], n[i + 5]); i += 6; break;
                        case 4: c.closePath(); break;
                    }
                }
                if (n[i++] === 0) {
                    c.fillStyle = fillStyle(n[i++]); c.fill(); gradient = null;
                } else {
                    c.strokeStyle = color(n[i]); c.lineWidth = n[i + 1]; c.stroke(); i += 2;
                }
                break;
            }
            case 21: {
                // A linear gradient for the next fill
                gradient = c.createLinearGradient(n[i], n[i + 1], n[i + 2], n[i + 3]);
                const stops = n[i + 4];
                i += 5;
                for (let k = 0; k < stops; k++) {
                    gradient.addColorStop(Math.max(0, Math.min(1, n[i])), color(n[i + 1]));
                    i += 2;
                }
                break;
            }
            case 22:
                // A soft shadow under an ellipse
                c.save();
                c.shadowColor = color(n[i + 4]); c.shadowBlur = n[i + 5];
                ellipse(c, n[i] + n[i + 2] / 2, n[i + 1] + n[i + 3] / 2, n[i + 2] / 2, n[i + 3] / 2);
                c.fillStyle = color(n[i + 4]); c.fill();
                c.restore();
                i += 6; break;
            case 30: c.clearRect(-1e6, -1e6, 2e6, 2e6); c.fillStyle = color(n[i++]); c.fillRect(-1e6, -1e6, 2e6, 2e6); break;
            default:
                console.error('XForms: unknown draw command', op);
                return;
        }
    }
}

// Pointers: the element under the pointer, then its ancestors, until one
// takes the press; it then gets the pointer's moves and release

function xids(target) {
    const ids = [];
    for (let element = target; element && element !== root; element = element.parentElement) {
        const id = element.dataset?.xid;
        if (id) {
            ids.push(Number(id));
        }
    }
    return ids;
}

function local(id, e) {
    const element = elements.get(id);
    const rect = element.getBoundingClientRect();
    const scaleX = element.offsetWidth ? rect.width / element.offsetWidth : 1;
    const scaleY = element.offsetHeight ? rect.height / element.offsetHeight : 1;
    return [(e.clientX - rect.left) / (scaleX || 1), (e.clientY - rect.top) / (scaleY || 1)];
}

function deviceType(e) {
    return (e.pointerType === 'touch') ? 0 : (e.pointerType === 'pen') ? 2 : 1;
}

function modifiers(e) {
    return (e.shiftKey ? 1 : 0) | (e.ctrlKey ? 2 : 0) | (e.altKey ? 4 : 0) | (e.metaKey ? 8 : 0);
}

function onPointerDown(e) {
    for (const id of xids(e.target)) {
        const [x, y] = local(id, e);
        if (exports.OnPointer(id, 1, e.pointerId, deviceType(e), x, y, true, modifiers(e))) {
            pointerOwners.set(e.pointerId, id);
            try { root.setPointerCapture(e.pointerId); } catch { }
            if (e.target.tagName !== 'INPUT' && e.target.tagName !== 'TEXTAREA') {
                e.preventDefault();
            }
            return;
        }
    }
}

function onPointerMove(e) {
    const owner = pointerOwners.get(e.pointerId);
    if (owner !== undefined) {
        const [x, y] = local(owner, e);
        exports.OnPointer(owner, 2, e.pointerId, deviceType(e), x, y, true, modifiers(e));
        return;
    }

    // A hovering mouse or pen
    if (e.pointerType !== 'touch') {
        const ids = xids(e.target);
        if (ids.length > 0) {
            const [x, y] = local(ids[0], e);
            exports.OnHover(ids[0], e.pointerId, deviceType(e), x, y, modifiers(e));
        }
    }
}

function onPointerEnd(e, phase) {
    const owner = pointerOwners.get(e.pointerId);
    if (owner === undefined) {
        return;
    }
    pointerOwners.delete(e.pointerId);
    try { root.releasePointerCapture(e.pointerId); } catch { }
    const [x, y] = local(owner, e);
    exports.OnPointer(owner, phase, e.pointerId, deviceType(e), x, y, true, modifiers(e));
}

function onWheel(e) {
    // One notch is 1.0, as on the other platforms (48 DIPs)
    const delta = -e.deltaY / (e.deltaMode === 0 ? 100 : 1);
    for (const id of xids(e.target)) {
        const [x, y] = local(id, e);
        if (exports.OnWheel(id, x, y, delta, modifiers(e))) {
            e.preventDefault();
            return;
        }
    }
}

// Scrolling

export function listenScroll(id) {
    const element = elements.get(id);
    element.addEventListener('scroll', () => exports.OnScroll(id, element.scrollLeft, element.scrollTop), { passive: true });
}

export function scrollTo(id, left, top, smooth) {
    elements.get(id).scrollTo({ left, top, behavior: smooth ? 'smooth' : 'instant' });
}

// Text entry

export function listenInput(id, isSingleLine) {
    const element = elements.get(id);
    element.addEventListener('input', () => exports.OnInput(id));
    element.addEventListener('select', () => exports.OnSelectionChanged(id));
    element.addEventListener('keyup', () => exports.OnSelectionChanged(id));
    element.addEventListener('focus', () => exports.OnFocusChanged(id, true));
    element.addEventListener('blur', () => exports.OnFocusChanged(id, false));
    if (isSingleLine) {
        element.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                exports.OnSubmit(id);
            }
        });
    }
}

export function getValue(id) {
    return elements.get(id).value;
}

export function setValue(id, value) {
    const element = elements.get(id);
    if (element.value !== value) {
        element.value = value;
    }
}

export function getSelection(id) {
    const element = elements.get(id);
    return [element.selectionStart ?? 0, element.selectionEnd ?? 0];
}

export function setSelection(id, start, end) {
    elements.get(id).setSelectionRange(start, end);
}

export function focus(id) {
    elements.get(id)?.focus({ preventScroll: true });
}

// Where an element is in the viewport: [x, y, width, height]
export function getRect(id) {
    const rect = elements.get(id).getBoundingClientRect();
    return [rect.left, rect.top, rect.width, rect.height];
}

// Overlays (popups) go above the page, in order

export function addOverlay(id) {
    root.appendChild(elements.get(id));
}

export function listenPress(id) {
    elements.get(id).addEventListener('pointerdown', (e) => {
        if (e.target === elements.get(id)) {
            exports.OnBackdropPressed(id);
        }
    });
}

// Fonts

export async function registerFont(family, url) {
    const face = new FontFace(family, `url(${url})`);
    document.fonts.add(await face.load());
    exports.OnFontLoaded();
}

// The page

export function setPageBackground(css) {
    document.documentElement.style.background = css;
    document.body.style.background = css;
}

export function openUrl(url) {
    window.open(url, '_blank', 'noopener');
}

export function getQueryParameter(name) {
    return new URLSearchParams(window.location.search).get(name);
}

export function listenVisibility() {
    document.addEventListener('visibilitychange', () => exports.OnVisibilityChanged(document.visibilityState === 'hidden'));
    window.addEventListener('pagehide', () => exports.OnVisibilityChanged(true));
}

// Services

export async function clipboardRead() {
    return await navigator.clipboard.readText();
}

export async function clipboardWrite(text) {
    await navigator.clipboard.writeText(text);
}

export function storageGet(key) {
    try { return window.localStorage.getItem(key); } catch { return null; }
}

export function storageSet(key, value) {
    try {
        if (value === null) {
            window.localStorage.removeItem(key);
        } else {
            window.localStorage.setItem(key, value);
        }
    } catch { }
}

// Removes every key with the prefix
export function storageClear(prefix) {
    try {
        for (const key of Object.keys(window.localStorage)) {
            if (key.startsWith(prefix)) {
                window.localStorage.removeItem(key);
            }
        }
    } catch { }
}

export function canShare() {
    return typeof navigator.share === 'function';
}

export async function share(title, text, url) {
    try {
        await navigator.share({ title: title || undefined, text: text || undefined, url: url || undefined });
    } catch { }
}

export function vibrate(milliseconds) {
    return (typeof navigator.vibrate === 'function') && navigator.vibrate(milliseconds);
}

export function isOnline() {
    return navigator.onLine;
}

export function listenOnline() {
    window.addEventListener('online', () => exports.OnOnlineChanged());
    window.addEventListener('offline', () => exports.OnOnlineChanged());
}

const pickedFiles = [];

// Asks for files; the names of those picked (none when canceled)
export function pickFiles(accept, multiple) {
    return new Promise((resolve) => {
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = accept;
        input.multiple = multiple;
        input.addEventListener('change', () => {
            pickedFiles.length = 0;
            pickedFiles.push(...input.files);
            resolve(pickedFiles.map((file) => file.name).join('\n'));
        });
        input.addEventListener('cancel', () => resolve(''));
        input.click();
    });
}

const pickedBytes = new Map();

export async function loadPickedFile(index) {
    const bytes = new Uint8Array(await pickedFiles[index].arrayBuffer());
    pickedBytes.set(index, bytes);
    return bytes.length;
}

export function copyPickedFile(index, view) {
    view.set(pickedBytes.get(index));
    pickedBytes.delete(index);
}

// Saves bytes as a download
export function saveFile(name, bytes) {
    const url = URL.createObjectURL(new Blob([bytes]));
    const link = document.createElement('a');
    link.href = url;
    link.download = name;
    document.body.appendChild(link);
    link.click();
    link.remove();
    setTimeout(() => URL.revokeObjectURL(url), 10000);
}

// Keyboard and focus: keys go to the focused element (or, with nothing
// focused, the root); .NET routes them to the view and its ancestors

function focusedId() {
    for (let element = document.activeElement; element && element !== document.body; element = element.parentElement) {
        const id = element.dataset?.xid;
        if (id) {
            return Number(id);
        }
    }
    return 0;
}

export function listenKeyboard() {
    // A focus ring for elements that take focus from the keyboard
    const style = document.createElement('style');
    style.textContent = '[data-xid]:focus { outline: none; } [data-xid][tabindex]:focus-visible { outline: 2px solid currentColor; outline-offset: 2px; } [data-xid]::placeholder { color: var(--xforms-placeholder); } @keyframes xforms-spin { to { transform: rotate(360deg); } }';
    document.head.appendChild(style);

    document.addEventListener('keydown', (e) => {
        const tag = document.activeElement?.tagName;
        const isTextEntry = tag === 'INPUT' || tag === 'TEXTAREA';
        // Text entries handle their own keys, except Tab and Escape
        if (isTextEntry && e.key !== 'Tab' && e.key !== 'Escape') {
            return;
        }
        if (exports.OnKey(focusedId(), e.key, modifiers(e), true, e.shiftKey && e.key === 'Tab')) {
            e.preventDefault();
        }
    });
    document.addEventListener('keyup', (e) => {
        const tag = document.activeElement?.tagName;
        if (tag !== 'INPUT' && tag !== 'TEXTAREA') {
            exports.OnKey(focusedId(), e.key, modifiers(e), false, false);
        }
    });
    root.addEventListener('focusin', (e) => {
        const id = Number(e.target.dataset?.xid ?? 0);
        if (id) {
            exports.OnFocusChanged(id, true);
        }
    });
    root.addEventListener('focusout', (e) => {
        const id = Number(e.target.dataset?.xid ?? 0);
        if (id) {
            exports.OnFocusChanged(id, false);
        }
    });
}

// History: one entry stands for the app's back stack, so the browser's Back
// goes back in the app

export function pushHistory() {
    history.pushState({ xforms: true }, '');
}

// Taking the entry away (when the app has gone back on its own) isn't a
// Back for the app
let isPoppingHistory = false;

export function popHistory() {
    isPoppingHistory = true;
    history.back();
}

export function listenHistory() {
    window.addEventListener('popstate', () => {
        if (isPoppingHistory) {
            isPoppingHistory = false;
            return;
        }
        exports.OnHistoryBack();
    });
}

// Accessible elements for lightweight views: a click (as a screen reader
// sends) activates the view

export function listenClick(id) {
    elements.get(id).addEventListener('click', (e) => {
        exports.OnAccessibilityClick(id);
        e.stopPropagation();
    });
}

// Pull-to-refresh: a touch pulling a list down past its top

// Pulls down from the top of a scrolled element: touches (which go on
// while the element scrolls), and mouse or pen drags
export function listenPull(id) {
    const element = elements.get(id);
    let start = null;
    const begin = (y) => {
        start = (element.scrollTop <= 0) ? y : null;
    };
    const move = (y) => {
        if (start !== null) {
            exports.OnPull(id, Math.max(0, y - start));
        }
    };
    const end = () => {
        if (start !== null) {
            start = null;
            exports.OnPullReleased(id);
        }
    };
    element.addEventListener('touchstart', (e) => begin(e.touches[0].clientY), { passive: true });
    element.addEventListener('touchmove', (e) => move(e.touches[0].clientY), { passive: true });
    element.addEventListener('touchend', end);
    element.addEventListener('touchcancel', end);
    element.addEventListener('pointerdown', (e) => {
        if (e.pointerType !== 'touch') {
            begin(e.clientY);
        }
    });
    element.addEventListener('pointermove', (e) => {
        if (e.pointerType !== 'touch') {
            move(e.clientY);
        }
    });
    element.addEventListener('pointerup', (e) => {
        if (e.pointerType !== 'touch') {
            end();
        }
    });
}

export function getScroll(id) {
    const element = elements.get(id);
    return [element.scrollLeft, element.scrollTop];
}
