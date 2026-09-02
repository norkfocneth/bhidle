// 2.5D Orthographic Camera Controller for 400x400 World
class GameCamera {
  constructor(viewportWidth, viewportHeight) {
    this.x = 2000;
    this.y = 2000;
    this.targetX = 2000;
    this.targetY = 2000;
    this.viewportWidth = viewportWidth;
    this.viewportHeight = viewportHeight;

    this.zoom = 1.0;
    this.targetZoom = 1.0;
    this.smoothFactor = 0.12;
  }

  resize(w, h) {
    this.viewportWidth = w;
    this.viewportHeight = h;
    // Set fixed orthographic scale so player is ~6-8% screen height
    const baseDim = Math.min(w, h);
    this.zoom = baseDim / 750; // Visible area is roughly 57x32 equivalent
    this.targetZoom = this.zoom;
  }

  follow(targetX, targetY) {
    this.targetX = targetX;
    this.targetY = targetY;
  }

  update(dt) {
    this.x += (this.targetX - this.x) * this.smoothFactor;
    this.y += (this.targetY - this.y) * this.smoothFactor;
  }

  applyTransform(ctx) {
    ctx.save();
    ctx.translate(this.viewportWidth / 2, this.viewportHeight / 2);
    ctx.scale(this.zoom, this.zoom);
    ctx.translate(-this.x, -this.y);
  }

  restoreTransform(ctx) {
    ctx.restore();
  }

  screenToWorld(screenX, screenY) {
    const relX = (screenX - this.viewportWidth / 2) / this.zoom;
    const relY = (screenY - this.viewportHeight / 2) / this.zoom;
    return {
      x: this.x + relX,
      y: this.y + relY
    };
  }

  worldToScreen(worldX, worldY) {
    const screenX = (worldX - this.x) * this.zoom + this.viewportWidth / 2;
    const screenY = (worldY - this.y) * this.zoom + this.viewportHeight / 2;
    return {
      x: screenX,
      y: screenY
    };
  }
}

window.GameCamera = GameCamera;
