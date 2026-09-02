// 2.5D Mobile HUD & UI Manager (Clean & Responsive)
class UIManager {
  constructor() {
    this.timerText = document.getElementById('hud-timer-text');
    this.leaderboardRows = document.getElementById('leaderboard-rows');
    this.myTerritoryPct = document.getElementById('my-territory-pct');
    this.myColorSwatch = document.getElementById('my-color-swatch');
    this.boostMeterFill = document.getElementById('boost-meter-fill');
    this.feedbackContainer = document.getElementById('floating-feedback-container');

    // Results screen
    this.resultsScreen = document.getElementById('results-screen');
    this.resTerritory = document.getElementById('res-territory');
    this.resRank = document.getElementById('res-rank');
    this.resKills = document.getElementById('res-kills');
    this.resScore = document.getElementById('res-score');
    this.resultsHeader = document.getElementById('results-header');
    this.resultsTitle = document.getElementById('results-title');

    this.playAgainBtn = document.getElementById('play-again-btn');
    if (this.playAgainBtn) {
      this.playAgainBtn.addEventListener('click', () => {
        this.hideGameOver();
        if (window.game) {
          window.game.startNewMatch();
        }
      });
    }
  }

  updateHUD(territoryPct, kills, remainingTime, boostEnergy = 100) {
    // 1. Timer Display (MM:SS)
    if (this.timerText && remainingTime !== undefined) {
      const m = Math.floor(remainingTime / 60);
      const s = Math.floor(remainingTime % 60);
      this.timerText.textContent = `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
    }

    // 2. MY TERRITORY Card
    if (this.myTerritoryPct) {
      this.myTerritoryPct.textContent = `${territoryPct.toFixed(1)}%`;
    }
    if (this.myColorSwatch && window.game) {
      this.myColorSwatch.style.backgroundColor = window.game.selectedColor;
    }

    // 3. Lightning Boost Energy SVG Ring
    if (this.boostMeterFill) {
      const circumference = 276.46;
      const offset = circumference - (boostEnergy / 100) * circumference;
      this.boostMeterFill.style.strokeDashoffset = offset;
    }
  }

  updateLeaderboard(allPlayers) {
    if (!this.leaderboardRows) return;

    const sorted = [...allPlayers].sort((a, b) => b.territoryPercentage - a.territoryPercentage);
    this.leaderboardRows.innerHTML = '';

    for (let i = 0; i < sorted.length; i++) {
      const p = sorted[i];
      const row = document.createElement('div');
      row.className = `lead-row ${p.isLocal ? 'is-local' : ''}`;
      row.style.backgroundColor = p.color;

      row.innerHTML = `
        <div class="lead-left">
          <span class="lead-rank-badge">${i + 1}</span>
          <span class="lead-name">${p.name}</span>
        </div>
        <div class="lead-right">
          <span class="lead-pct">${p.territoryPercentage.toFixed(1)}%</span>
          ${i === 0 ? '<span class="lead-crown">👑</span>' : ''}
        </div>
      `;

      this.leaderboardRows.appendChild(row);
    }
  }

  showFloatingFeedback(text, worldX, worldY) {
    if (!this.feedbackContainer || !window.game) return;

    const screenPos = window.game.camera.worldToScreen(worldX, worldY);
    const elem = document.createElement('div');
    elem.className = 'float-text';
    elem.textContent = text;
    elem.style.left = `${screenPos.x}px`;
    elem.style.top = `${screenPos.y - 40}px`;

    this.feedbackContainer.appendChild(elem);
    setTimeout(() => {
      elem.remove();
    }, 1200);
  }

  showKillBanner(killerName, victimName) {
    if (killerName === 'Arnav' || killerName === (window.game ? window.game.playerName : 'Arnav')) {
      if (window.game && window.game.player) {
        this.showFloatingFeedback('Kill !', window.game.player.x, window.game.player.y);
      }
    }
  }

  showGameOver(isVictory, territoryPct, rank, kills, score, winnerName = 'Arnav', winnerPct = 0) {
    if (!this.resultsScreen) return;

    this.resultsScreen.classList.remove('hidden');
    if (this.resultsHeader) {
      this.resultsHeader.textContent = isVictory ? 'VICTORY 👑' : 'MATCH FINISHED';
    }
    if (this.resultsTitle) {
      this.resultsTitle.textContent = isVictory
        ? `ARENA CONQUERED! (${territoryPct.toFixed(1)}%)`
        : `WINNER: ${winnerName} (${winnerPct.toFixed(1)}%)`;
    }
    if (this.resTerritory) this.resTerritory.textContent = `${territoryPct.toFixed(1)}%`;
    if (this.resRank) this.resRank.textContent = `#${rank} ${rank === 1 ? '👑' : ''}`;
    if (this.resKills) this.resKills.textContent = kills;
    if (this.resScore) this.resScore.textContent = score;

    if (window.soundManager) {
      if (isVictory) {
        window.soundManager.playKill();
      } else {
        window.soundManager.playDeath();
      }
    }
  }

  hideGameOver() {
    if (this.resultsScreen) {
      this.resultsScreen.classList.add('hidden');
    }
  }
}

window.UIManager = UIManager;
