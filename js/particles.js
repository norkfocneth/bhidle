// 2.5D Paper Confetti and Particle Physics Engine
class ParticleSystem {
  constructor() {
    this.particles = [];
  }

  spawnBurst(x, y, color, count = 35) {
    const colors = [color, '#ffffff', '#ffd60a', '#00d2ff', '#ff2d55'];
    for (let i = 0; i < count; i++) {
      const angle = Math.random() * Math.PI * 2;
      const speed = 80 + Math.random() * 260;
      this.particles.push({
        x: x,
        y: y,
        vx: Math.cos(angle) * speed,
        vy: Math.sin(angle) * speed,
        width: 6 + Math.random() * 8,
        height: 8 + Math.random() * 12,
        rot: Math.random() * Math.PI * 2,
        rotSpeed: (Math.random() - 0.5) * 12,
        flip: Math.random() * Math.PI,
        flipSpeed: (Math.random() - 0.5) * 14,
        color: colors[Math.floor(Math.random() * colors.length)],
        alpha: 1.0,
        decay: 0.8 + Math.random() * 0.9,
        gravity: 120
      });
    }
  }

  spawnDeathExplosion(x, y, color) {
    this.spawnBurst(x, y, color, 45);
  }

  spawnTrailDust(x, y, color) {
    if (Math.random() > 0.4) return;
    this.particles.push({
      x: x + (Math.random() - 0.5) * 6,
      y: y + (Math.random() - 0.5) * 6,
      vx: (Math.random() - 0.5) * 20,
      vy: (Math.random() - 0.5) * 20,
      width: 4,
      height: 4,
      rot: 0,
      rotSpeed: 0,
      flip: 0,
      flipSpeed: 0,
      color: color,
      alpha: 0.6,
      decay: 2.2,
      gravity: 0
    });
  }

  update(dt) {
    for (let i = this.particles.length - 1; i >= 0; i--) {
      const p = this.particles[i];
      p.x += p.vx * dt;
      p.y += p.vy * dt;
      p.vy += p.gravity * dt;
      p.rot += p.rotSpeed * dt;
      p.flip += p.flipSpeed * dt;
      p.alpha -= p.decay * dt;

      if (p.alpha <= 0) {
        this.particles.splice(i, 1);
      }
    }
  }

  render(ctx) {
    for (const p of this.particles) {
      ctx.save();
      ctx.translate(p.x, p.y);
      ctx.rotate(p.rot);
      ctx.scale(Math.cos(p.flip), 1);
      ctx.globalAlpha = Math.max(0, p.alpha);
      ctx.fillStyle = p.color;
      ctx.fillRect(-p.width / 2, -p.height / 2, p.width, p.height);
      ctx.restore();
    }
  }

  clear() {
    this.particles = [];
  }
}

window.particleSystem = new ParticleSystem();
