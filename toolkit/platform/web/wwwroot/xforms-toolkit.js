// The toolkit's services in the browser: the clipboard, local storage,
// sharing, vibration, online status and files. .NET calls these through
// ToolkitJs.

let exports = null;

export async function init() {
    const runtime = await globalThis.getDotnetRuntime(0);
    exports = (await runtime.getAssemblyExports('XForms.Toolkit.Web')).XForms.Web.ToolkitJs;
}

export function openUrl(url) {
    window.open(url, '_blank', 'noopener');
}

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

