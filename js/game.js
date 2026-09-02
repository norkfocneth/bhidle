// TERRAGRAV - Master 2.5D Arena Game Engine (5-Min Match & Strict Empty Respawn)
class Game {
  constructor() {
    this.canvas = document.getElementById('game-canvas');
    this.ctx = this.canvas.getContext('2d');

    // 2400x2400 Circular Fortress Arena
    this.worldSize = 2400;
    this.arenaRadius = 980;
    this.centerX = this.worldSize / 2; // 1200
    this.centerY = this.worldSize / 2; // 1200

    this.grid = new TerritoryGrid(this.worldSize, 300);
    this.camera = new GameCamera(window.innerWidth, window.innerHeight);
    this.ui = new UIManager();
    window.uiManager = this.ui;

    this.dpr = window.devicePixelRatio || 1;

    this.player = null;
    this.bots = [];
    this.allEntities = [];

    this.gameState = 'PLAYING';
    this.matchDuration = 300; // 5 MINUTES MATCH (05:00)
    this.matchTime = this.matchDuration;
    this.lastTime = performance.now();

    // Input state
    this.inputDir = { x: 0.707, y: -0.707 };
    this.keysPressed = {};

    this.selectedColor = '#1a73e8';
    this.playerName = 'Arnav';

    // Environmental Decorations (Trees, Rocks, Bushes, Clouds)
    this.decorations = this.generateDecorations();
    this.clouds = this.generateClouds();

    this.initEventListeners();
    this.resize();
    this.startNewMatch();

    requestAnimationFrame((t) => this.loop(t));
  }

  generateDecorations() {
    const decs = [];
    const seedPoints = 48;
    const r = this.arenaRadius - 25;

    for (let i = 0; i < seedPoints; i++) {
      const a = (i / seedPoints) * Math.PI * 2 + (Math.random() * 0.05);
      const dist = r - Math.random() * 30;
      const x = this.centerX + Math.cos(a) * dist;
      const y = this.centerY + Math.sin(a) * dist;
      const type = i % 2 === 0 ? 'tree' : 'bush';

      decs.push({ x, y, type, scale: 0.9 + Math.random() * 0.3 });
    }

    // Scattered mossy stone blocks on the arena floor (matching reference)
    const innerRocks = [
      { x: this.centerX - 180, y: this.centerY - 140 },
      { x: this.centerX - 160, y: this.centerY + 120 },
      { x: this.centerX - 100, y: this.centerY + 130 },
      { x: this.centerX + 150, y: this.centerY - 120 },
      { x: this.centerX + 140, y: this.centerY + 140 },
      { x: this.centerX + 90,  y: this.centerY - 160 },
      { x: this.centerX - 140, y: this.centerY - 320 },
      { x: this.centerX + 120, y: this.centerY + 320 }
    ];

    innerRocks.forEach(rk => {
      decs.push({ x: rk.x, y: rk.y, type: 'rock', scale: 1.0 });
    });

    return decs;
  }

  generateClouds() {
    const clouds = [];
    for (let i = 0; i < 18; i++) {
      clouds.push({
        x: Math.random() * 3400 - 500,
        y: Math.random() * 3400 - 500,
        speed: 8 + Math.random() * 12,
        size: 90 + Math.random() * 120,
        alpha: 0.3 + Math.random() * 0.3
      });
    }
    return clouds;
  }

  resize() {
    this.dpr = window.devicePixelRatio || 1;
    this.canvas.width = Math.floor(window.innerWidth * this.dpr);
    this.canvas.height = Math.floor(window.innerHeight * this.dpr);
    this.canvas.style.width = `${window.innerWidth}px`;
    this.canvas.style.height = `${window.innerHeight}px`;
    this.camera.resize(window.innerWidth, window.innerHeight);
  }

  initEventListeners() {
    window.addEventListener('resize', () => this.resize());

    // Keyboard Input
    window.addEventListener('keydown', (e) => {
      this.keysPressed[e.key.toLowerCase()] = true;
      this.keysPressed[e.code] = true;
      if (e.code === 'Space' && this.player) {
        this.player.setBoost(true);
      }
      this.updateKeyboardDirection();
    });

    window.addEventListener('keyup', (e) => {
      this.keysPressed[e.key.toLowerCase()] = false;
      this.keysPressed[e.code] = false;
      if (e.code === 'Space' && this.player) {
        this.player.setBoost(false);
      }
      this.updateKeyboardDirection();
    });

    // Ultra-Smooth Pointer & Mouse Steering
    const handlePointer = (clientX, clientY) => {
      if (this.gameState === 'PLAYING' && this.player && this.player.isAlive) {
        const worldPos = this.camera.screenToWorld(clientX, clientY);
        const dx = worldPos.x - this.player.x;
        const dy = worldPos.y - this.player.y;
        if (dx * dx + dy * dy > 30) {
          const mag = Math.sqrt(dx * dx + dy * dy);
          this.inputDir = { x: dx / mag, y: dy / mag };
        }
      }
    };

    window.addEventListener('mousemove', (e) => handlePointer(e.clientX, e.clientY));
    window.addEventListener('pointermove', (e) => handlePointer(e.clientX, e.clientY));
    window.addEventListener('pointerdown', (e) => handlePointer(e.clientX, e.clientY));
    window.addEventListener('touchmove', (e) => {
      if (e.touches.length > 0) {
        handlePointer(e.touches[0].clientX, e.touches[0].clientY);
      }
    }, { passive: true });

    // Boost Action Button
    const boostBtn = document.getElementById('boost-btn');
    if (boostBtn) {
      boostBtn.addEventListener('mousedown', () => { if (this.player) this.player.setBoost(true); });
      window.addEventListener('mouseup', () => { if (this.player) this.player.setBoost(false); });
      boostBtn.addEventListener('touchstart', (e) => { e.preventDefault(); if (this.player) this.player.setBoost(true); }, { passive: false });
      boostBtn.addEventListener('touchend', () => { if (this.player) this.player.setBoost(false); });
    }
  }

  updateKeyboardDirection() {
    let dx = 0, dy = 0;
    if (this.keysPressed['w'] || this.keysPressed['arrowup']) dy -= 1;
    if (this.keysPressed['s'] || this.keysPressed['arrowdown']) dy += 1;
    if (this.keysPressed['a'] || this.keysPressed['arrowleft']) dx -= 1;
    if (this.keysPressed['d'] || this.keysPressed['arrowright']) dx += 1;

    if (dx !== 0 || dy !== 0) {
      const mag = Math.sqrt(dx * dx + dy * dy);
      this.inputDir = { x: dx / mag, y: dy / mag };
    }
  }

  startNewMatch() {
    this.grid.reset();
    this.bots = [];
    this.allEntities = [];
    this.matchTime = this.matchDuration; // 300s = 5 mins

    const startRad = 95; // Starting circular base radius

    // 1. Initialize Local Player P1 (Arnav - Blue) strictly inside the bottom center of the arena
    const p1X = this.centerX;
    const p1Y = this.centerY + 450;

    this.player = new Player(1, 'Arnav', '#1a73e8', p1X, p1Y, true);
    this.grid.registerPlayer(1, '#1a73e8');
    this.grid.claimCircle(p1X, p1Y, startRad, 1);
    this.allEntities.push(this.player);

    // 2. Initialize 7 Bots strictly inside the circular arena (Exact reference positions)
    const botProfiles = [
      { name: 'Rohan',   color: '#4caf50', x: this.centerX - 350, y: this.centerY - 350 }, // Top Left
      { name: 'Vihaan',  color: '#ffb300', x: this.centerX - 50,  y: this.centerY - 450 }, // Top Center
      { name: 'Kabir',   color: '#e53935', x: this.centerX + 350, y: this.centerY - 350 }, // Top Right
      { name: 'Yash',    color: '#8e24aa', x: this.centerX + 450, y: this.centerY - 20 },  // Middle Right
      { name: 'Dev',     color: '#00acc1', x: this.centerX - 450, y: this.centerY - 20 },  // Middle Left
      { name: 'Reyansh', color: '#e91e63', x: this.centerX - 350, y: this.centerY + 350 }, // Bottom Left
      { name: 'Shiva',   color: '#fb8c00', x: this.centerX + 350, y: this.centerY + 350 }  // Bottom Right
    ];

    botProfiles.forEach((cfg, idx) => {
      const bot = new BotPlayer(idx + 2, cfg.name, cfg.color, cfg.x, cfg.y);
      this.grid.registerPlayer(bot.id, bot.color);
      this.grid.claimCircle(cfg.x, cfg.y, startRad, bot.id);
      this.bots.push(bot);
      this.allEntities.push(bot);
    });

    this.camera.x = this.player.x;
    this.camera.y = this.player.y;
    this.gameState = 'PLAYING';
    this.inputDir = { x: 0.707, y: -0.707 };
  }

  update(dt) {
    if (this.gameState !== 'PLAYING') return;

    // 5-Minute Match Timer Countdown
    this.matchTime -= dt;
    if (this.matchTime <= 0) {
      this.matchTime = 0;
      this.endMatch();
      return;
    }

    // Clouds Drift
    for (const c of this.clouds) {
      c.x += c.speed * dt;
      if (c.x > this.worldSize + 400) c.x = -400;
    }

    // Update Player & Camera
    if (this.player.isAlive) {
      this.player.update(dt, this.inputDir, this.grid, this.centerX, this.centerY, this.arenaRadius);
      this.player.territoryPercentage = this.grid.getPlayerPercentage(this.player.id);
      this.camera.follow(this.player.x, this.player.y);
    }

    // Update All 7 Autonomous Bots
    for (const bot of this.bots) {
      if (bot.isAlive) {
        bot.updateAI(dt, this.grid, this.allEntities, this.centerX, this.centerY, this.arenaRadius);
        bot.territoryPercentage = this.grid.getPlayerPercentage(bot.id);
      }
    }

    this.camera.update(dt);
    window.particleSystem.update(dt);

    // Collisions
    this.checkCollisions();

    // UI Updates
    if (this.player) {
      this.ui.updateHUD(
        this.player.territoryPercentage,
        this.player.kills,
        this.matchTime,
        this.player.boostEnergy
      );
    }
    this.ui.updateLeaderboard(this.allEntities);
  }

  checkCollisions() {
    for (const attacker of this.allEntities) {
      if (!attacker.isAlive) continue;

      for (const victim of this.allEntities) {
        // CAN NEVER DIE TO OWN TRAIL
        if (attacker.id === victim.id || !victim.isAlive) continue;

        // 1. Trail Cutting Check (Only outside base trail can be cut)
        if (victim.trail.length >= 2) {
          for (let i = 0; i < victim.trail.length; i++) {
            const tp = victim.trail[i];
            const dist2 = (attacker.x - tp.x) ** 2 + (attacker.y - tp.y) ** 2;
            const hitRadius = attacker.radius + 8;

            if (dist2 < hitRadius * hitRadius) {
              this.handleElimination(attacker, victim);
              break;
            }
          }
        }

        // 2. Home Territory Defense Rule (Inside your own territory, YOU ARE INVINCIBLE!)
        // If an enemy enters your territory and collides head-to-head, the intruder is eliminated!
        const distHead2 = (attacker.x - victim.x) ** 2 + (attacker.y - victim.y) ** 2;
        const headHitRadius = attacker.radius + victim.radius;

        if (distHead2 < headHitRadius * headHitRadius) {
          const victimHome = this.grid.getOwnerAtWorld(victim.x, victim.y) === victim.id;
          const attackerHome = this.grid.getOwnerAtWorld(attacker.x, attacker.y) === attacker.id;

          if (victimHome && !attackerHome) {
            // Attacker intruded into victim's home! Intruder dies!
            this.handleElimination(victim, attacker);
          } else if (attackerHome && !victimHome) {
            // Victim intruded into attacker's home! Victim dies!
            this.handleElimination(attacker, victim);
          }
        }
      }
    }
  }

  handleElimination(killer, victim) {
    victim.isAlive = false;
    victim.trail = [];

    if (killer && killer.id !== victim.id) {
      // KILLER ABSORBS VICTIM'S ENTIRE TERRITORY! (Instantly converts to killer's color)
      const absorbedCount = this.grid.absorbPlayerTerritory(victim.id, killer.id);
      killer.kills++;
      killer.score += 500;
      killer.territoryPercentage = this.grid.getPlayerPercentage(killer.id);
      victim.territoryPercentage = 0;

      if (killer.isLocal && window.uiManager) {
        window.uiManager.showFloatingFeedback('Territory Captured! 💥', killer.x, killer.y);
        if (window.soundManager) window.soundManager.playKill();
      }
    } else {
      this.grid.eliminatePlayerTerritory(victim.id);
    }

    window.particleSystem.spawnBurst(victim.x, victim.y, victim.color, 45);
    if (window.soundManager) window.soundManager.playDeath();

    this.ui.showKillBanner(killer.name, victim.name);

    setTimeout(() => this.respawnEntity(victim), 3500);
  }

  // Find pure empty/unclaimed neutral space for respawn
  findEmptySpawnLocation(startRad = 85) {
    const res = this.grid.resolution;
    const cellSize = this.grid.cellSize;
    const rCells = Math.ceil(startRad / cellSize);
    const r2 = rCells * rCells;
    const maxDistWorld = this.arenaRadius - startRad - 40;

    const emptyCandidates = [];

    // Scan the circular arena in a grid grid pattern
    for (let gy = rCells + 2; gy < res - rCells - 2; gy += 4) {
      for (let gx = rCells + 2; gx < res - rCells - 2; gx += 4) {
        const wx = (gx + 0.5) * cellSize;
        const wy = (gy + 0.5) * cellSize;

        const dx = wx - this.centerX;
        const dy = wy - this.centerY;
        if (dx * dx + dy * dy > maxDistWorld * maxDistWorld) continue;

        // Check if the entire circular footprint is 100% UNCLAIMED (owner === -1)
        let isStrictlyEmpty = true;
        for (let dyC = -rCells; dyC <= rCells; dyC += 2) {
          for (let dxC = -rCells; dxC <= rCells; dxC += 2) {
            if (dxC * dxC + dyC * dyC <= r2) {
              if (this.grid.getOwner(gx + dxC, gy + dyC) !== -1) {
                isStrictlyEmpty = false;
                break;
              }
            }
          }
          if (!isStrictlyEmpty) break;
        }

        if (isStrictlyEmpty) {
          emptyCandidates.push({ x: wx, y: wy });
        }
      }
    }

    if (emptyCandidates.length === 0) {
      return null; // NO EMPTY SPACE LEFT ON MAP! CANNOT RESPAWN!
    }

    // Return a random spot from the verified empty spots
    return emptyCandidates[Math.floor(Math.random() * emptyCandidates.length)];
  }

  respawnEntity(entity) {
    const emptySpot = this.findEmptySpawnLocation(85);

    if (!emptySpot) {
      // IF NO EMPTY SPACE EXISTS ON THE MAP, DO NOT RESPAWN OVER ANYONE ELSE'S BASE!
      entity.isAlive = false;
      entity.trail = [];
      console.log(`[TERRAGRAV] No empty neutral space available on map for ${entity.name}. Spawning skipped.`);
      return;
    }

    entity.x = emptySpot.x;
    entity.y = emptySpot.y;
    entity.isAlive = true;
    entity.trail = [];
    this.grid.claimCircle(entity.x, entity.y, 85, entity.id);
  }

  endMatch() {
    this.gameState = 'GAMEOVER';

    // 5-Minute Timer Finished: Winner is player with HIGHEST territory percentage!
    const sorted = [...this.allEntities].sort((a, b) => b.territoryPercentage - a.territoryPercentage);
    const winner = sorted[0];
    const isPlayerWinner = (winner.id === this.player.id);
    const playerRank = sorted.findIndex(p => p.id === this.player.id) + 1;
    const playerScore = Math.round(this.player.territoryPercentage * 1000) + (this.player.kills * 250);

    this.ui.showGameOver(
      isPlayerWinner,
      this.player.territoryPercentage,
      playerRank,
      this.player.kills,
      playerScore,
      winner.name,
      winner.territoryPercentage
    );
  }

  render() {
    const ctx = this.ctx;
    ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

    if (!this.player) return;

    ctx.save();
    ctx.scale(this.dpr, this.dpr);

    // Apply 2.5D Camera Viewport Transform (Smooth Player Follow)
    this.camera.applyTransform(ctx);

    // 1. Outside Sky & Clouds
    this.renderSkyEnvironment(ctx);

    // 2. Circular Checkered Slate Arena Floor
    this.renderArenaFloor(ctx);

    // 3. 3D Raised Organic Territory Surfaces
    this.grid.render(ctx);

    // 4. Environmental Trees, Rocks, Flowers
    this.renderDecorations(ctx);

    // 5. Glossy 3D Character Trails
    for (const entity of this.allEntities) {
      if (entity.isAlive) entity.renderTrail(ctx);
    }

    // 6. Particle VFX
    window.particleSystem.render(ctx);

    // 7. Cute 3D Cube Robot Characters
    for (const entity of this.allEntities) {
      entity.renderHead(ctx);
    }

    // 8. 3D Modular Stone Perimeter Wall with Glowing Cyan Lights
    this.renderStonePerimeterWall(ctx);

    this.camera.restoreTransform(ctx);
    ctx.restore();
  }

  renderSkyEnvironment(ctx) {
    ctx.fillStyle = '#64b5f6';
    ctx.fillRect(-1000, -1000, this.worldSize + 2000, this.worldSize + 2000);

    ctx.fillStyle = 'rgba(255, 255, 255, 0.4)';
    for (const c of this.clouds) {
      ctx.beginPath();
      ctx.arc(c.x, c.y, c.size * 0.5, 0, Math.PI * 2);
      ctx.arc(c.x + c.size * 0.35, c.y - c.size * 0.15, c.size * 0.4, 0, Math.PI * 2);
      ctx.arc(c.x - c.size * 0.35, c.y - c.size * 0.1, c.size * 0.35, 0, Math.PI * 2);
      ctx.fill();
    }
  }

  renderArenaFloor(ctx) {
    const r = this.arenaRadius;

    // 1. Ambient Drop Shadow on Sky
    ctx.save();
    ctx.fillStyle = 'rgba(15, 23, 42, 0.45)';
    ctx.beginPath();
    ctx.arc(this.centerX + 20, this.centerY + 35, r + 24, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();

    // 2. Circular Checkered Arena Floor
    ctx.save();
    ctx.beginPath();
    ctx.arc(this.centerX, this.centerY, r, 0, Math.PI * 2);
    ctx.clip();

    ctx.fillStyle = '#1e2638';
    ctx.fillRect(0, 0, this.worldSize, this.worldSize);

    // Checkered slate grid tiles
    const tileSize = 55;
    for (let x = 0; x < this.worldSize; x += tileSize) {
      for (let y = 0; y < this.worldSize; y += tileSize) {
        if ((Math.floor(x / tileSize) + Math.floor(y / tileSize)) % 2 === 0) {
          ctx.fillStyle = '#171e2e';
          ctx.fillRect(x, y, tileSize, tileSize);
        }
      }
    }

    ctx.restore();
  }

  renderDecorations(ctx) {
    for (const d of this.decorations) {
      ctx.save();
      ctx.translate(d.x, d.y);
      ctx.scale(d.scale, d.scale);

      if (d.type === 'tree') {
        // Pine Tree
        ctx.fillStyle = 'rgba(0, 0, 0, 0.3)';
        ctx.beginPath();
        ctx.ellipse(3, 8, 14, 7, 0, 0, Math.PI * 2);
        ctx.fill();

        ctx.fillStyle = '#1b5e20';
        ctx.beginPath();
        ctx.arc(0, -5, 14, 0, Math.PI * 2);
        ctx.fill();

        ctx.fillStyle = '#2e7d32';
        ctx.beginPath();
        ctx.arc(0, -13, 11, 0, Math.PI * 2);
        ctx.fill();

        ctx.fillStyle = '#43a047';
        ctx.beginPath();
        ctx.arc(0, -19, 7, 0, Math.PI * 2);
        ctx.fill();
      } else if (d.type === 'bush') {
        // Flower Bush
        ctx.fillStyle = '#2e7d32';
        ctx.beginPath();
        ctx.arc(0, 0, 10, 0, Math.PI * 2);
        ctx.arc(-6, 2, 7, 0, Math.PI * 2);
        ctx.arc(6, 2, 7, 0, Math.PI * 2);
        ctx.fill();

        ctx.fillStyle = '#fbbf24';
        ctx.beginPath();
        ctx.arc(0, -3, 3, 0, Math.PI * 2);
        ctx.fill();
      } else {
        // Mossy Stone Block
        ctx.fillStyle = 'rgba(0, 0, 0, 0.35)';
        ctx.fillRect(-7, -3, 18, 14);

        ctx.fillStyle = '#475569';
        ctx.fillRect(-9, -9, 16, 12);

        ctx.fillStyle = '#64748b';
        ctx.fillRect(-9, -9, 16, 3.5);

        ctx.fillStyle = '#2e7d32';
        ctx.fillRect(-9, -5, 4, 3);
      }

      ctx.restore();
    }
  }

  renderStonePerimeterWall(ctx) {
    const r = this.arenaRadius;
    const count = 64;
    const angleStep = (Math.PI * 2) / count;

    for (let i = 0; i < count; i++) {
      const a = i * angleStep;
      const wx = this.centerX + Math.cos(a) * r;
      const wy = this.centerY + Math.sin(a) * r;

      ctx.save();
      ctx.translate(wx, wy);
      ctx.rotate(a + Math.PI / 2);

      // Stone Block
      ctx.fillStyle = '#334155';
      ctx.fillRect(-22, -14, 44, 28);

      ctx.fillStyle = '#475569';
      ctx.fillRect(-20, -12, 40, 8);

      // Cyan Lights
      if (i % 4 === 0) {
        ctx.fillStyle = '#00e5ff';
        ctx.shadowColor = '#00e5ff';
        ctx.shadowBlur = 12;
        ctx.fillRect(-6, -6, 12, 12);

        ctx.fillStyle = '#ffffff';
        ctx.fillRect(-3, -3, 6, 6);
      }

      ctx.restore();
    }
  }

  loop(currentTime) {
    const dt = Math.min(0.1, (currentTime - this.lastTime) / 1000);
    this.lastTime = currentTime;

    this.update(dt);
    this.render();

    requestAnimationFrame((t) => this.loop(t));
  }
}

window.onload = () => {
  window.game = new Game();
};
