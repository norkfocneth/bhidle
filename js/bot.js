// Autonomous AI Bot Opponents for TERRAGRAV (Circular Arena)
class BotPlayer extends Player {
  constructor(id, name, color, x, y) {
    super(id, name, color, x, y, false);
    this.speed = 185 + Math.random() * 25;
    this.turnSpeed = 7.5;

    this.state = 'EXPAND'; // 'EXPAND', 'HUNT', 'RETREAT'
    this.stateTimer = 0;
    this.loopDuration = 2.2 + Math.random() * 2.5;
    this.loopDirection = Math.random() > 0.5 ? 1 : -1;
    this.homePos = { x: x, y: y };
  }

  updateAI(dt, grid, allPlayers, arenaCenterX = 1200, arenaCenterY = 1200, arenaRadius = 970) {
    if (!this.isAlive) return;

    this.stateTimer += dt;
    let desiredDir = { x: Math.cos(this.targetAngle), y: Math.sin(this.targetAngle) };

    // 1. Check for nearby vulnerable enemy trails to hunt
    let closestTrailPoint = null;
    let minTrailDist2 = 160 * 160;

    for (const other of allPlayers) {
      if (other.id === this.id || !other.isAlive || other.trail.length < 3) continue;

      for (let i = 0; i < other.trail.length - 2; i++) {
        const p = other.trail[i];
        const dist2 = (this.x - p.x) ** 2 + (this.y - p.y) ** 2;
        if (dist2 < minTrailDist2) {
          minTrailDist2 = dist2;
          closestTrailPoint = p;
        }
      }
    }

    if (closestTrailPoint && Math.random() < 0.85) {
      this.state = 'HUNT';
      const angleToTarget = Math.atan2(closestTrailPoint.y - this.y, closestTrailPoint.x - this.x);
      desiredDir = { x: Math.cos(angleToTarget), y: Math.sin(angleToTarget) };
    } else {
      // 2. State Machine: Expand vs Retreat
      if (this.trail.length > 26 || this.stateTimer > this.loopDuration * 1.5) {
        this.state = 'RETREAT';
      }

      if (this.state === 'RETREAT') {
        const angleToHome = Math.atan2(this.homePos.y - this.y, this.homePos.x - this.x);
        desiredDir = { x: Math.cos(angleToHome), y: Math.sin(angleToHome) };

        if (!this.isOutside) {
          this.state = 'EXPAND';
          this.stateTimer = 0;
          this.loopDuration = 2.0 + Math.random() * 2.5;
          this.loopDirection = Math.random() > 0.5 ? 1 : -1;
          this.homePos = { x: this.x, y: this.y };
        }
      } else {
        // Expand in a smooth curved arc
        const currentA = Math.atan2(desiredDir.y, desiredDir.x);
        const nextA = currentA + (Math.PI / this.loopDuration) * this.loopDirection * dt;
        desiredDir = { x: Math.cos(nextA), y: Math.sin(nextA) };
      }
    }

    // 3. Circular Arena Boundary Avoidance (Steer inward if near stone wall)
    const dx = this.x - arenaCenterX;
    const dy = this.y - arenaCenterY;
    const distFromCenter = Math.sqrt(dx * dx + dy * dy);

    if (distFromCenter > arenaRadius - 140) {
      const angleToCenter = Math.atan2(arenaCenterY - this.y, arenaCenterX - this.x);
      desiredDir = {
        x: desiredDir.x * 0.3 + Math.cos(angleToCenter) * 0.7,
        y: desiredDir.y * 0.3 + Math.sin(angleToCenter) * 0.7
      };
    }

    this.update(dt, desiredDir, grid, arenaCenterX, arenaCenterY, arenaRadius);
  }
}

window.BotPlayer = BotPlayer;
