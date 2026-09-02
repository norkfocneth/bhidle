// High-Performance Rock-Solid Territory Grid with Global Exterior-Flood Hole Elimination
class TerritoryGrid {
  constructor(worldSize = 2400, resolution = 300) {
    this.worldSize = worldSize;
    this.resolution = resolution;
    this.cellSize = worldSize / resolution; // 8px per cell
    this.centerX = worldSize / 2;
    this.centerY = worldSize / 2;
    this.arenaRadius = 980;

    this.grid = new Int16Array(resolution * resolution);
    this.grid.fill(-1);

    this.playerColors = new Map();
    this.playerCellCounts = new Map();

    this.offscreenCanvas = document.createElement('canvas');
    this.offscreenCanvas.width = resolution;
    this.offscreenCanvas.height = resolution;
    this.offscreenCtx = this.offscreenCanvas.getContext('2d', { willReadFrequently: true });
    this.isDirty = true;
  }

  reset() {
    this.grid.fill(-1);
    this.playerColors.clear();
    this.playerCellCounts.clear();
    this.isDirty = true;
  }

  registerPlayer(id, color) {
    this.playerColors.set(id, color);
    if (!this.playerCellCounts.has(id)) {
      this.playerCellCounts.set(id, 0);
    }
  }

  worldToGrid(wx, wy) {
    const gx = Math.floor(wx / this.cellSize);
    const gy = Math.floor(wy / this.cellSize);
    return {
      gx: Math.max(0, Math.min(this.resolution - 1, gx)),
      gy: Math.max(0, Math.min(this.resolution - 1, gy)),
      inBounds: gx >= 0 && gx < this.resolution && gy >= 0 && gy < this.resolution
    };
  }

  gridToWorld(gx, gy) {
    return {
      x: (gx + 0.5) * this.cellSize,
      y: (gy + 0.5) * this.cellSize
    };
  }

  getOwner(gx, gy) {
    if (gx < 0 || gx >= this.resolution || gy < 0 || gy >= this.resolution) return -1;
    return this.grid[gy * this.resolution + gx];
  }

  getOwnerAtWorld(wx, wy) {
    const { gx, gy, inBounds } = this.worldToGrid(wx, wy);
    if (!inBounds) return -1;
    return this.getOwner(gx, gy);
  }

  claimCell(gx, gy, playerId) {
    if (gx < 0 || gx >= this.resolution || gy < 0 || gy >= this.resolution) return;
    const idx = gy * this.resolution + gx;
    const oldOwner = this.grid[idx];
    if (oldOwner === playerId) return;

    if (oldOwner !== -1) {
      const oldCount = this.playerCellCounts.get(oldOwner) || 0;
      if (oldCount > 0) this.playerCellCounts.set(oldOwner, oldCount - 1);
    }

    this.grid[idx] = playerId;
    const newCount = this.playerCellCounts.get(playerId) || 0;
    this.playerCellCounts.set(playerId, newCount + 1);
    this.isDirty = true;
  }

  claimCircle(wx, wy, radiusWorld, playerId) {
    const { gx: cx, gy: cy } = this.worldToGrid(wx, wy);
    const radCells = Math.ceil(radiusWorld / this.cellSize);
    const rad2 = radCells * radCells;

    const minX = Math.max(0, cx - radCells);
    const maxX = Math.min(this.resolution - 1, cx + radCells);
    const minY = Math.max(0, cy - radCells);
    const maxY = Math.min(this.resolution - 1, cy + radCells);

    for (let y = minY; y <= maxY; y++) {
      for (let x = minX; x <= maxX; x++) {
        const d2 = (x - cx) ** 2 + (y - cy) ** 2;
        if (d2 <= rad2) {
          this.claimCell(x, y, playerId);
        }
      }
    }
  }

  captureTrailEnclosure(playerId, trailWorldPoints) {
    if (trailWorldPoints.length < 2) return 0;

    const gridPoints = trailWorldPoints.map(p => this.worldToGrid(p.x, p.y));

    // 1. Solidly rasterize and claim the trail on the grid
    for (let i = 0; i < gridPoints.length - 1; i++) {
      const p0 = gridPoints[i];
      const p1 = gridPoints[i + 1];

      this.rasterizeSuperCover(
        p0.gx, p0.gy,
        p1.gx, p1.gy,
        (gx, gy) => {
          // Thick 3x3 brush for trail
          for (let dy = -1; dy <= 1; dy++) {
            for (let dx = -1; dx <= 1; dx++) {
              this.claimCell(gx + dx, gy + dy, playerId);
            }
          }
        }
      );
    }

    // 2. Eliminate all enclosed holes, cavities, lakes, and islands inside the player's perimeter
    const filledCount = this.fillEnclosedTerritory(playerId);
    return filledCount + gridPoints.length;
  }

  // Complete Topological Enclosed Hole & Lake Elimination Algorithm
  fillEnclosedTerritory(playerId) {
    const res = this.resolution;
    const total = res * res;
    const visited = new Uint8Array(total);
    const queueX = new Int16Array(total);
    const queueY = new Int16Array(total);
    let head = 0, tail = 0;

    const centerGx = this.centerX / this.cellSize;
    const centerGy = this.centerY / this.cellSize;
    const arenaRadLimit2 = (this.arenaRadius / this.cellSize) ** 2;

    // 1. Seed exterior flood-fill from the outer boundary of the entire world
    const enqueue = (gx, gy) => {
      if (gx < 0 || gx >= res || gy < 0 || gy >= res) return;
      const idx = gy * res + gx;
      if (visited[idx]) return;
      visited[idx] = 1;

      // Cannot cross player's territory barrier
      if (this.grid[idx] !== playerId) {
        queueX[tail] = gx;
        queueY[tail] = gy;
        tail++;
      }
    };

    // Seed all 4 outer grid edges
    for (let x = 0; x < res; x++) {
      enqueue(x, 0);
      enqueue(x, res - 1);
    }
    for (let y = 0; y < res; y++) {
      enqueue(0, y);
      enqueue(res - 1, y);
    }

    // 4-way BFS expansion across exterior open arena
    const dx = [1, -1, 0, 0];
    const dy = [0, 0, 1, -1];

    while (head < tail) {
      const cx = queueX[head];
      const cy = queueY[head];
      head++;

      for (let d = 0; d < 4; d++) {
        const nx = cx + dx[d];
        const ny = cy + dy[d];

        if (nx >= 0 && nx < res && ny >= 0 && ny < res) {
          const nIdx = ny * res + nx;
          if (!visited[nIdx]) {
            visited[nIdx] = 1;
            if (this.grid[nIdx] !== playerId) {
              queueX[tail] = nx;
              queueY[tail] = ny;
              tail++;
            }
          }
        }
      }
    }

    // 2. Any cell inside the circular arena that was NOT reached by exterior flood-fill is 100% ENCLOSED!
    // Instantly claim every single one of them!
    let filledCount = 0;
    for (let gy = 0; gy < res; gy++) {
      for (let gx = 0; gx < res; gx++) {
        const idx = gy * res + gx;
        const dist2 = (gx - centerGx) ** 2 + (gy - centerGy) ** 2;

        if (dist2 <= arenaRadLimit2) {
          if (!visited[idx] && this.grid[idx] !== playerId) {
            this.claimCell(gx, gy, playerId);
            filledCount++;
          }
        }
      }
    }

    return filledCount;
  }

  rasterizeSuperCover(x0, y0, x1, y1, plot) {
    let dx = Math.abs(x1 - x0);
    let dy = Math.abs(y1 - y0);
    let x = x0;
    let y = y0;
    let n = 1 + dx + dy;
    let x_inc = (x1 > x0) ? 1 : -1;
    let y_inc = (y1 > y0) ? 1 : -1;
    let error = dx - dy;
    dx *= 2;
    dy *= 2;

    for (; n > 0; --n) {
      plot(x, y);
      if (error > 0) {
        x += x_inc;
        error -= dy;
      } else if (error < 0) {
        y += y_inc;
        error += dx;
      } else {
        x += x_inc;
        y += y_inc;
        error += dx - dy;
        --n;
        plot(x, y);
      }
    }
  }

  getPlayerPercentage(playerId) {
    const count = this.playerCellCounts.get(playerId) || 0;
    const arenaCells = Math.floor(Math.PI * (this.resolution * 0.41) ** 2);
    return Math.min(100, (count / arenaCells) * 100);
  }

  eliminatePlayerTerritory(playerId) {
    const res = this.resolution;
    for (let i = 0; i < res * res; i++) {
      if (this.grid[i] === playerId) {
        this.grid[i] = -1;
      }
    }
    this.playerCellCounts.set(playerId, 0);
    this.isDirty = true;
  }

  absorbPlayerTerritory(victimId, killerId) {
    const res = this.resolution;
    let convertedCount = 0;
    for (let i = 0; i < res * res; i++) {
      if (this.grid[i] === victimId) {
        this.grid[i] = killerId;
        convertedCount++;
      }
    }
    this.playerCellCounts.set(victimId, 0);
    const prevKillerCount = this.playerCellCounts.get(killerId) || 0;
    this.playerCellCounts.set(killerId, prevKillerCount + convertedCount);
    this.isDirty = true;

    // Also seal any enclosed gaps in the new merged territory
    this.fillEnclosedTerritory(killerId);

    return convertedCount;
  }

  updateOffscreenBuffer() {
    if (!this.isDirty) return;

    const ctx = this.offscreenCtx;
    const res = this.resolution;
    const imgData = ctx.createImageData(res, res);
    const data = imgData.data;

    const rgbLookup = {};
    for (const [id, hex] of this.playerColors.entries()) {
      rgbLookup[id] = this.hexToRgb(hex);
    }

    for (let y = 0; y < res; y++) {
      for (let x = 0; x < res; x++) {
        const idx = y * res + x;
        const owner = this.grid[idx];
        const pIdx = idx * 4;

        if (owner !== -1 && rgbLookup[owner]) {
          const c = rgbLookup[owner];
          data[pIdx] = c.r;
          data[pIdx + 1] = c.g;
          data[pIdx + 2] = c.b;
          data[pIdx + 3] = 245;
        } else {
          data[pIdx + 3] = 0;
        }
      }
    }

    ctx.putImageData(imgData, 0, 0);
    this.isDirty = false;
  }

  render(ctx) {
    this.updateOffscreenBuffer();
    ctx.save();
    ctx.imageSmoothingEnabled = false;

    // 2.5D Drop shadow under territory
    ctx.shadowColor = 'rgba(0, 0, 0, 0.45)';
    ctx.shadowBlur = 12;
    ctx.shadowOffsetY = 6;
    ctx.drawImage(this.offscreenCanvas, 0, 0, this.worldSize, this.worldSize);
    ctx.restore();
  }

  hexToRgb(hex) {
    hex = hex.replace('#', '');
    if (hex.length === 3) hex = hex.split('').map(c => c + c).join('');
    const num = parseInt(hex, 16);
    return {
      r: (num >> 16) & 255,
      g: (num >> 8) & 255,
      b: num & 255
    };
  }
}

window.TerritoryGrid = TerritoryGrid;
