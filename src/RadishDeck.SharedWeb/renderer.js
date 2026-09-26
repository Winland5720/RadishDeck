(function (global) {
  function Renderer(options) {
    options = options || {};
    this.root = options.root;
    this.canvas = options.canvas || { width: 1920, height: 1080 };
    this.onActivate = options.onActivate || function () {};
    this.nodes = new Map();
  }
  Renderer.prototype.setScale = function (scale) {
    this.root.style.width = this.canvas.width + 'px';
    this.root.style.height = this.canvas.height + 'px';
    this.root.style.transform = 'scale(' + scale + ')';
    this.root.style.transformOrigin = 'top left';
  };
  Renderer.prototype.render = function (elements) {
    var seen = new Set();
    (elements || []).forEach(function (element) {
      if (!element || !element.id || !element.layout) return;
      seen.add(element.id);
      var node = this.nodes.get(element.id);
      if (!node) {
        node = document.createElement(element.type === 'Button' ? 'button' : 'div');
        node.dataset.elementId = element.id;
        node.className = 'rd-element rd-' + String(element.type || 'element').toLowerCase();
        node.addEventListener('click', function (event) {
          this.onActivate(element, event);
        }.bind(this));
        this.root.appendChild(node);
        this.nodes.set(element.id, node);
      }
      var l = element.layout;
      node.style.left = l.x + 'px'; node.style.top = l.y + 'px';
      node.style.width = l.width + 'px'; node.style.height = l.height + 'px';
      node.textContent = (element.content && element.content.text) || element.name || element.type || 'Element';
      var a = element.appearance || {};
      if (a.background) node.style.background = a.background;
      if (a.color) node.style.color = a.color;
      if (a.border) node.style.border = a.border;
      if (a.radius != null) node.style.borderRadius = a.radius + 'px';
      node.__renderElement = element;
    }, this);
    Array.from(this.nodes.keys()).forEach(function (id) {
      if (!seen.has(id)) { var n = this.nodes.get(id); n.remove(); this.nodes.delete(id); }
    }, this);
  };
  global.RadishSharedRenderer = Renderer;
})(window);
