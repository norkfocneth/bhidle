// 2.5D Animated Collectable Power-ups System
class PowerUp {
  constructor(x, y, type = 'shield') {
    this.x = x;
    this.y = y;
    this.type = type; // 'shield', 'speed', 'bomb'
    this.radius = 20;
    this.collected = false;

    this.bobTimer = Math.random() * Math.PI * 2;
    this.rotAngle = 0;

    // Color definitions
    this.colorConfig = {
      shield: { primary: '#00e5ff', secondary: '#0088ff', icon: '🛡️' },
      speed: { primary: '#ffd600', secondary: '#ff6d00', icon: '⚡' },
      bomb: { primary: '#e040fb', secondary: '#7c4dff', icon: '💣' }
    };
  }

  update(dt) {
    this.bobTimer += dt * 3.5;
    this.rotAngle += dt * 2.0;
  }

  checkCollision(player) {
    if (this.collected || !player.isAlive) return false;
    const dx = player.x - this.x;
    const dy = player.y - this.y;
    const dist2 = dx * dx + dy * dy;
    const hitRadius = this.radius + player.radius;

    if (dist2 <= hitRadius * hitRadius) {
      this.collected = true;
      this.applyEffect(player);
      return true;
    }
    return false;
  }

  applyEffect(player) {
    const cfg = this.colorConfig[this.type];
    if (window.particleSystem) {
      window.particleSystem.spawnBurst(this.x, this.y, cfg.primary, 30);
    }
    if (window.soundManager) {
      window.soundManager.playCapture();
    }

    if (this.type === 'shield') {
      player.shields = (player.shields || 0) + 1;
      player.activateShield(6.0); // 6 seconds auto-shield or banked
      if (player.isLocal && window.uiManager) {
        window.uiManager.showFloatingFeedback('Shield +1 🛡️', this.x, this.y);
      }
    } else if (this.type === 'speed') {
      player.boostEnergy = player.maxBoostEnergy;
      player.speed = player.baseSpeed * 1.6;
      setTimeout(() => { if (player.isAlive) player.speed = player.baseSpeed; }, 3500);
      if (player.isLocal && window.uiManager) {
        window.uiManager.showFloatingFeedback('Mega Speed ⚡', this.x, this.y);
      }
    } else if (this.type === 'bomb') {
      if (window.game && window.game.grid) {
        window.game.grid.claimCircle(player.x, player.y, 140, player.id);
      }
      if (player.isLocal && window.uiManager) {
        window.uiManager.showFloatingFeedback('Land Bomb 💣', this.x, this.y);
      }
    }
  }

  render(ctx) {
    if (this.collected) return;

    const cfg = this.colorConfig[this.type];
    const bobOffset = Math.sin(this.bobTimer) * 6;

    ctx.save();
    ctx.translate(this.x, this.y + bobOffset);

    // 1. 2.5D Ground Shadow
    ctx.save();
    ctx.fillStyle = 'rgba(0, 0, 0, 0.35)';
    ctx.beginPath();
    ctx.ellipse(0, 18 - bobOffset, 16, 8, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();

    // 2. Outer Glowing Ring
    ctx.save();
    ctx.rotate(this.rotAngle);
    ctx.strokeStyle = cfg.primary;
    ctx.lineWidth = 3;
    ctx.shadowColor = cfg.primary;
    ctx.shadowBlur = 14;
    ctx.beginPath();
    ctx.arc(0, 0, 18, 0, Math.PI * 2);
    ctx.stroke();

    // Orbiting sparkle dots
    for (let i = 0; i < 3; i++) {
      const a = (i * Math.PI * 2) / 3;
      ctx.fillStyle = '#ffffff';
      ctx.beginPath();
      ctx.arc(Math.cos(a) * 18, Math.sin(a) * 18, 2.5, 0, Math.PI * 2);
      ctx.fill();
    }
    ctx.restore();

    // 3. Central Glossy Orb
    const grad = ctx.createRadialGradient(-4, -4, 2, 0, 0, 14);
    grad.addColorStop(0, '#ffffff');
    grad.addColorStop(0.4, cfg.primary);
    grad.addColorStop(1, cfg.secondary);

    ctx.fillStyle = grad;
    ctx.beginPath();
    ctx.arc(0, 0, 13, 0, Math.PI * 2);
    ctx.fill();

    // 4. Icon
    ctx.font = '13px sans-serif';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(cfg.icon, 0, 0);

    ctx.restore();
  }
}

window.PowerUp = PowerUp;
