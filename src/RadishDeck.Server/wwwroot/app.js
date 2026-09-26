const canvas = { width: 1920, height: 1080 };
const root = document.getElementById('deck');
const renderer = new RadishSharedRenderer({ root, canvas, onActivate: element => fetch('/execute', { method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify({ elementId: element.id }) }) });
function scale() { renderer.setScale(Math.min(root.parentElement.clientWidth / canvas.width, window.innerHeight / canvas.height)); }
async function load() {
  try {
    const response = await fetch('/deck', { cache: 'no-store' });
    if (!response.ok) throw new Error('Deck request failed: ' + response.status);
    const deck = await response.json();
    const page = (deck.pages || [])[0];
    renderer.render(page ? page.elements : []);
    scale();
  } catch (error) {
    console.warn('Runtime deck refresh failed; keeping current view', error);
  }
}
window.addEventListener('resize', scale);
load();
window.setInterval(load, 750);
