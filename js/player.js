// 2.5D Stylized Cute Cube Robot Character
class Player {
  constructor(id, name, color, x, y, isLocal = true) {
    this.id = id;
    this.name = name;
    this.color = color;
    this.isLocal = isLocal;

    this.x = x;
    this.y = y;
    this.cubeSize = 28;
    this.radius = 16;
    this.angle = -Math.PI / 4;
    this.targetAngle = -Math.PI / 4;
    this.baseSpeed = 210;
    this.speed = this.baseSpeed;
    this.turnSpeed = 10.0;

    // Boost Mechanics
    this.isBoosting = false;
    this.boostEnergy = 100;
    this.maxBoostEnergy = 100;

    this.isAlive = true;
    this.kills = 0;
    this.score = 0;
    this.territoryPercentage = 0;

    this.trail = [];
    this.isOutside = false;
    this.wasOutside = false;
    this.minTrailSampleDist = 6;

    this.walkCycle = 0;
  }

  setBoost(active) {
    if (active && this.boostEnergy > 10) {
      this.isBoosting = true;
    } else {
      this.isBoosting = false;
    }
  }

  update(dt, inputDir, grid, arenaCenterX = 1200, arenaCenterY = 1200, arenaRadius = 970) {
    if (!this.isAlive) return;

    // Handle Boost Energy
    if (this.isBoosting && this.boostEnergy > 0) {
      this.speed = this.baseSpeed * 1.5;
      this.boostEnergy = Math.max(0, this.boostEnergy - dt * 45);
      if (this.boostEnergy <= 0) this.isBoosting = false;
      if (window.particleSystem) {
        window.particleSystem.spawnTrailDust(this.x, this.y, '#ffffff');
      }
    } else {
      this.speed = this.baseSpeed;
      this.boostEnergy = Math.min(this.maxBoostEnergy, this.boostEnergy + dt * 20);
    }

    // Smooth turning
    if (inputDir && (inputDir.x !== 0 || inputDir.y !== 0)) {
      this.targetAngle = Math.atan2(inputDir.y, inputDir.x);
    }

    let diff = this.targetAngle - this.angle;
    while (diff < -Math.PI) diff += Math.PI * 2;
    while (diff > Math.PI) diff -= Math.PI * 2;
    this.angle += diff * Math.min(1.0, this.turnSpeed * dt);

    // Continuous translation
    this.x += Math.cos(this.angle) * this.speed * dt;
    this.y += Math.sin(this.angle) * this.speed * dt;
    this.walkCycle += dt * 14;

    // Circular Arena Boundary Clamp (Keeps player strictly inside the stone arena)
    const dx = this.x - arenaCenterX;
    const dy = this.y - arenaCenterY;
    const dist = Math.sqrt(dx * dx + dy * dy);
    const maxAllowedDist = arenaRadius - this.radius - 10;

    if (dist > maxAllowedDist) {
      this.x = arenaCenterX + (dx / dist) * maxAllowedDist;
      this.y = arenaCenterY + (dy / dist) * maxAllowedDist;
    }

    // Territory state & trail sampling
    const currentOwner = grid.getOwnerAtWorld(this.x, this.y);
    this.isOutside = (currentOwner !== this.id);

    if (this.isOutside) {
      if (this.trail.length === 0) {
        this.trail.push({ x: this.x, y: this.y });
      } else {
        const lastP = this.trail[this.trail.length - 1];
        const dist2 = (this.x - lastP.x) ** 2 + (this.y - lastP.y) ** 2;
        if (dist2 >= this.minTrailSampleDist ** 2) {
          this.trail.push({ x: this.x, y: this.y });
          if (window.particleSystem) {
            window.particleSystem.spawnTrailDust(this.x, this.y, this.color);
          }
        }
      }
    } else {
      if (this.trail.length > 2) {
        this.trail.push({ x: this.x, y: this.y });
        const captured = grid.captureTrailEnclosure(this.id, this.trail);
        if (captured > 0) {
          if (window.soundManager) window.soundManager.playCapture();
          if (this.isLocal && window.uiManager) {
            window.uiManager.showFloatingFeedback('Great !', this.x, this.y);
          }
        }
      }
      this.trail = [];
    }

    this.wasOutside = this.isOutside;
  }

  renderTrail(ctx) {
    if (this.trail.length < 2) return;

    ctx.save();
    // 2.5D Drop shadow under ribbon
    ctx.shadowColor = 'rgba(0, 0, 0, 0.25)';
    ctx.shadowBlur = 6;
    ctx.shadowOffsetY = 4;

    ctx.beginPath();
    ctx.moveTo(this.trail[0].x, this.trail[0].y);
    for (let i = 1; i < this.trail.length; i++) {
      ctx.lineTo(this.trail[i].x, this.trail[i].y);
    }
    if (this.isOutside) {
      ctx.lineTo(this.x, this.y);
    }

    ctx.strokeStyle = this.color;
    ctx.lineWidth = 14;
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';
    ctx.stroke();

    // Glossy specular highlight
    ctx.strokeStyle = this.shadeColor(this.color, 25);
    ctx.lineWidth = 4;
    ctx.stroke();

    ctx.restore();
  }

  renderHead(ctx) {
    if (!this.isAlive) return;

    ctx.save();
    ctx.translate(this.x, this.y);

    // 1. Soft 3D Ground Shadow
    ctx.save();
    ctx.fillStyle = 'rgba(0, 0, 0, 0.35)';
    ctx.beginPath();
    ctx.ellipse(6, 12, this.cubeSize * 0.85, this.cubeSize * 0.45, -Math.PI / 8, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();

    // 2. Cute Little Black Feet
    const legOffset = Math.sin(this.walkCycle) * 3;
    ctx.fillStyle = '#1e293b';
    ctx.beginPath();
    ctx.arc(-7, 10 + legOffset, 3, 0, Math.PI * 2);
    ctx.arc(7, 10 - legOffset, 3, 0, Math.PI * 2);
    ctx.arc(-2, 12 - legOffset, 2.5, 0, Math.PI * 2);
    ctx.arc(12, 9 + legOffset, 2.5, 0, Math.PI * 2);
    ctx.fill();

    // 3. Cute 3D Isometric Robot Cube
    this.drawCuteRobotCube(ctx, 0, 0, this.cubeSize, this.color);

    // 4. Floating Name Tag above head
    this.drawNameTag(ctx, 0, -this.cubeSize - 14, this.name, this.color);

    ctx.restore();
  }

  drawCuteRobotCube(ctx, cx, cy, size, baseColor) {
    const s = size * 0.7;
    const h = size * 0.7;

    const topColor = this.shadeColor(baseColor, 20);
    const leftColor = this.shadeColor(baseColor, -25);
    const rightColor = this.shadeColor(baseColor, -10);

    ctx.save();
    ctx.translate(cx, cy - 5);

    // Antenna on top with golden/yellow tip ball
    ctx.strokeStyle = '#1e293b';
    ctx.lineWidth = 2.2;
    ctx.beginPath();
    ctx.moveTo(0, -h);
    ctx.lineTo(2, -h - 10);
    ctx.stroke();

    ctx.fillStyle = '#ffb300';
    ctx.beginPath();
    ctx.arc(2, -h - 11, 3, 0, Math.PI * 2);
    ctx.fill();

    // TOP FACE
    ctx.fillStyle = topColor;
    ctx.beginPath();
    ctx.moveTo(0, -h);
    ctx.lineTo(s * 1.15, -h * 0.4);
    ctx.lineTo(0, h * 0.2);
    ctx.lineTo(-s * 1.15, -h * 0.4);
    ctx.closePath();
    ctx.fill();

    // LEFT FACE
    ctx.fillStyle = leftColor;
    ctx.beginPath();
    ctx.moveTo(-s * 1.15, -h * 0.4);
    ctx.lineTo(0, h * 0.2);
    ctx.lineTo(0, h * 1.2);
    ctx.lineTo(-s * 1.15, h * 0.6);
    ctx.closePath();
    ctx.fill();

    // RIGHT / FRONT FACE (With cute face eyes and smile!)
    ctx.fillStyle = rightColor;
    ctx.beginPath();
    ctx.moveTo(0, h * 0.2);
    ctx.lineTo(s * 1.15, -h * 0.4);
    ctx.lineTo(s * 1.15, h * 0.6);
    ctx.lineTo(0, h * 1.2);
    ctx.closePath();
    ctx.fill();

    // Cute Black Robot Eyes on front face
    ctx.fillStyle = '#0f172a';
    ctx.beginPath();
    ctx.arc(s * 0.35, h * 0.25, 2.5, 0, Math.PI * 2);
    ctx.arc(s * 0.8, h * 0.05, 2.5, 0, Math.PI * 2);
    ctx.fill();

    // Eye Highlights
    ctx.fillStyle = '#ffffff';
    ctx.beginPath();
    ctx.arc(s * 0.35 - 0.7, h * 0.25 - 0.7, 0.9, 0, Math.PI * 2);
    ctx.arc(s * 0.8 - 0.7, h * 0.05 - 0.7, 0.9, 0, Math.PI * 2);
    ctx.fill();

    ctx.restore();
  }

  drawNameTag(ctx, x, y, name, color) {
    ctx.save();
    ctx.font = 'bold 11px "Outfit", sans-serif';
    ctx.textAlign = 'center';

    ctx.fillStyle = '#000000';
    ctx.fillText(name, x + 1, y + 1);

    ctx.fillStyle = color;
    ctx.fillText(name, x, y);

    ctx.fillStyle = color;
    ctx.beginPath();
    ctx.moveTo(x - 4, y + 3);
    ctx.lineTo(x + 4, y + 3);
    ctx.lineTo(x, y + 7);
    ctx.closePath();
    ctx.fill();

    ctx.restore();
  }

  shadeColor(color, percent) {
    let num = parseInt(color.replace('#', ''), 16);
    let amt = Math.round(2.55 * percent);
    let R = (num >> 16) + amt;
    let G = (num >> 8 & 0x00FF) + amt;
    let B = (num & 0x0000FF) + amt;
    return '#' + (0x1000000 + (R < 255 ? R < 1 ? 0 : R : 255) * 0x10000 + (G < 255 ? G < 1 ? 0 : G : 255) * 0x100 + (B < 255 ? B < 1 ? 0 : B : 255)).toString(16).slice(1);
  }
}

window.Player = Player;
