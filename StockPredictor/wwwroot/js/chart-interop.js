window.stockCharts = {
    _instances: {},

    createPriceChart: function (canvasId, labels, close, ma5, ma10, ma50) {
        this._destroy(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        this._instances[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Close',
                        data: close,
                        borderColor: '#1a1a2e',
                        backgroundColor: 'rgba(26,26,46,0.08)',
                        borderWidth: 2,
                        pointRadius: 0,
                        fill: true,
                        tension: 0.3
                    },
                    {
                        label: 'MA 5',
                        data: ma5,
                        borderColor: '#4361ee',
                        borderWidth: 1.5,
                        pointRadius: 0,
                        borderDash: [4, 2],
                        tension: 0.3
                    },
                    {
                        label: 'MA 10',
                        data: ma10,
                        borderColor: '#2ec4b6',
                        borderWidth: 1.5,
                        pointRadius: 0,
                        borderDash: [4, 2],
                        tension: 0.3
                    },
                    {
                        label: 'MA 50',
                        data: ma50,
                        borderColor: '#e76f51',
                        borderWidth: 2,
                        pointRadius: 0,
                        tension: 0.3
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },
                plugins: {
                    legend: { position: 'top', labels: { boxWidth: 12, padding: 10, font: { size: 11 } } },
                    title: { display: true, text: 'Price & Moving Averages', font: { size: 14 } }
                },
                scales: {
                    x: { ticks: { maxTicksLimit: 10, font: { size: 10 } } },
                    y: { ticks: { font: { size: 10 } } }
                }
            }
        });
    },

    createRsiChart: function (canvasId, labels, rsi) {
        this._destroy(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        const overbought = labels.map(() => 70);
        const oversold = labels.map(() => 30);

        this._instances[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'RSI',
                        data: rsi,
                        borderColor: '#7209b7',
                        borderWidth: 2,
                        pointRadius: 0,
                        tension: 0.3
                    },
                    {
                        label: 'Overbought (70)',
                        data: overbought,
                        borderColor: '#e63946',
                        borderWidth: 1,
                        borderDash: [6, 3],
                        pointRadius: 0
                    },
                    {
                        label: 'Oversold (30)',
                        data: oversold,
                        borderColor: '#2a9d8f',
                        borderWidth: 1,
                        borderDash: [6, 3],
                        pointRadius: 0
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },
                plugins: {
                    legend: { position: 'top', labels: { boxWidth: 12, padding: 10, font: { size: 11 } } },
                    title: { display: true, text: 'RSI (14)', font: { size: 14 } }
                },
                scales: {
                    x: { ticks: { maxTicksLimit: 10, font: { size: 10 } } },
                    y: { min: 0, max: 100, ticks: { font: { size: 10 } } }
                }
            }
        });
    },

    createVolumeChart: function (canvasId, labels, volume) {
        this._destroy(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        this._instances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Volume',
                    data: volume,
                    backgroundColor: 'rgba(67,97,238,0.35)',
                    borderColor: '#4361ee',
                    borderWidth: 1,
                    borderRadius: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    title: { display: true, text: 'Volume', font: { size: 14 } }
                },
                scales: {
                    x: { ticks: { maxTicksLimit: 10, font: { size: 10 } } },
                    y: { ticks: { font: { size: 10 }, callback: function (v) { return (v / 1e6).toFixed(0) + 'M'; } } }
                }
            }
        });
    },

    createVolatilityChart: function (canvasId, labels, volatility) {
        this._destroy(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        this._instances[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Volatility',
                    data: volatility,
                    borderColor: '#e76f51',
                    backgroundColor: 'rgba(231,111,81,0.1)',
                    borderWidth: 2,
                    pointRadius: 0,
                    fill: true,
                    tension: 0.3
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    title: { display: true, text: 'Volatility', font: { size: 14 } }
                },
                scales: {
                    x: { ticks: { maxTicksLimit: 10, font: { size: 10 } } },
                    y: { ticks: { font: { size: 10 } } }
                }
            }
        });
    },

    _destroy: function (canvasId) {
        if (this._instances[canvasId]) {
            this._instances[canvasId].destroy();
            delete this._instances[canvasId];
        }
    },

    createEquityCurveChart: function (canvasId, labels, equity, startingCapital) {
        this._destroy(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        const baseline = labels.map(() => startingCapital);

        this._instances[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Portfolio',
                        data: equity,
                        borderColor: '#4361ee',
                        backgroundColor: 'rgba(67,97,238,0.08)',
                        borderWidth: 2,
                        pointRadius: 0,
                        fill: true,
                        tension: 0.3
                    },
                    {
                        label: 'Starting Capital',
                        data: baseline,
                        borderColor: '#adb5bd',
                        borderWidth: 1,
                        borderDash: [6, 3],
                        pointRadius: 0
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },
                plugins: {
                    legend: { position: 'top', labels: { boxWidth: 12, padding: 10, font: { size: 11 } } },
                    title: { display: true, text: 'Equity Curve', font: { size: 14 } }
                },
                scales: {
                    x: { title: { display: true, text: 'Trade #', font: { size: 10 } }, ticks: { maxTicksLimit: 12, font: { size: 10 } } },
                    y: { title: { display: true, text: '$', font: { size: 10 } }, ticks: { font: { size: 10 }, callback: function (v) { return '$' + v.toLocaleString(); } } }
                }
            }
        });
    },

    createTradeReturnsChart: function (canvasId, labels, returns) {
        this._destroy(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        const colors = returns.map(r => r >= 0 ? 'rgba(46,196,182,0.7)' : 'rgba(231,111,81,0.7)');
        const borders = returns.map(r => r >= 0 ? '#2ec4b6' : '#e76f51');

        this._instances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Return %',
                    data: returns,
                    backgroundColor: colors,
                    borderColor: borders,
                    borderWidth: 1,
                    borderRadius: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    title: { display: true, text: 'Per-Trade Returns', font: { size: 14 } }
                },
                scales: {
                    x: { title: { display: true, text: 'Trade #', font: { size: 10 } }, ticks: { maxTicksLimit: 15, font: { size: 10 } } },
                    y: { title: { display: true, text: '%', font: { size: 10 } }, ticks: { font: { size: 10 } } }
                }
            }
        });
    }
};
