// Simple non-module interop that uses global Chart (loaded before this script)
window.exchangeChartInterop = (function () {
    let chart = null;

    function toArray(points) {
        if (!points) return [];
        if (Array.isArray(points)) return points;
        if (typeof points === 'string') {
            try {
                const parsed = JSON.parse(points);
                if (Array.isArray(parsed)) return parsed;
                if (parsed && typeof parsed === 'object') return Object.values(parsed);
            }
            catch (e) {
                console.error('exchangeChartInterop: failed to parse points string', e);
                return [];
            }
        }
        if (typeof points === 'object') {
            if (typeof points.length === 'number') return Array.from(points);
            return Object.values(points);
        }
        return [];
    }

    function createChart(ctx, points, title) {
        const values = points.map(p => p.y);
        const minVal = Math.min(...values);
        const maxVal = Math.max(...values);
        const span = Math.max((maxVal - minVal) * 0.1, 0.0001);

        chart = new Chart(ctx, {
            type: 'line',
            data: {
                datasets: [{
                    label: title ?? 'Exchange Rate',
                    data: points,
                    parsing: { xAxisKey: 'x', yAxisKey: 'y' },
                    borderColor: 'rgba(75, 192, 192, 1)',
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    tension: 0.0,
                    pointRadius: 0,
                    pointBackgroundColor: 'rgba(255,255,255,1)',
                    pointHoverRadius: 5,
                    borderWidth: 2,
                    fill: false
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: { type: 'time', time: { unit: 'day', tooltipFormat: 'yyyy-MM-dd' }, display: true },
                    y: {
                        display: true,
                        min: Math.floor((minVal - span) * 100000) / 100000,
                        max: Math.ceil((maxVal + span) * 100000) / 100000
                    }
                }
            }
        });
    }

    function render(points) {
        try {
            console.log('exchangeChartInterop.render invoked with:', points);
            let parsed = points;
            if (typeof parsed === 'string') {
                try { parsed = JSON.parse(parsed); } catch (e) { parsed = points; }
            }

            // Determine payload shape. Support legacy array-of-points, or object { from, to, points }
            let arr = [];
            let title = null;

            if (Array.isArray(parsed)) {
                arr = parsed;
            }
            else if (parsed && typeof parsed === 'object') {
                if (Array.isArray(parsed.points)) {
                    arr = parsed.points;
                    const fromCode = parsed.from?.code ?? parsed.from?.name ?? null;
                    const toCode = parsed.to?.code ?? parsed.to?.name ?? null;
                    if (fromCode && toCode) title = `${fromCode} → ${toCode}`;
                    else if (parsed.from?.name && parsed.to?.name) title = `${parsed.from.name} → ${parsed.to.name}`;
                }
                else {
                    // fallback: use object values
                    arr = Object.values(parsed);
                }
            }

            console.log('exchangeChartInterop parsed array:', arr);

            const canvas = document.getElementById('exchangeChart');
            if (!canvas) {
                console.error('exchangeChartInterop: canvas element with id "exchangeChart" not found');
                return;
            }

            const ctx = canvas.getContext('2d');
            if (!ctx) {
                console.error('exchangeChartInterop: unable to get 2d context from canvas');
                return;
            }

            const normalized = arr.flatMap(item => {
                // item might itself be an array of points (e.g. when parsed object contained points array)
                if (Array.isArray(item)) return item;
                return [item];
            }).map(p => {
                const dateVal = p?.date ?? p?.x ?? p?.time;
                const parsedDate = new Date(dateVal);
                // use ISO date (yyyy-mm-dd) to avoid locale issues
                const label = (parsedDate && !isNaN(parsedDate.getTime())) ? parsedDate.toISOString().slice(0, 10) : (dateVal ?? '');
                const rate = Number(p?.rate ?? p?.y ?? p?.value);
                return { label, rate };
            }).filter(pt => typeof pt.label === 'string' && pt.label.length > 0 && Number.isFinite(pt.rate));

            if (normalized.length === 0) {
                console.warn('exchangeChartInterop: no valid data points to render', arr);
                return;
            }

            console.log('exchangeChartInterop normalized points:', normalized);

            // For time axis use {x,y} points where x is ISO date and y is numeric value
            const chartPoints = normalized.map(p => ({ x: p.label, y: p.rate }));

            // If a previous Chart instance exists it may be bound to a removed/replaced canvas.
            // Destroy it to ensure the new canvas is used and chart is recreated cleanly.
            if (chart) {
                try {
                    chart.destroy();
                }
                catch (e) {
                    console.warn('exchangeChartInterop: failed to destroy previous chart', e);
                }
                chart = null;
            }

            // create chart (always recreate to avoid references to replaced canvas)
            createChart(ctx, chartPoints, title ?? undefined);
            console.log('exchangeChartInterop: chart created');
        }
        catch (e) {
            console.error('exchangeChartInterop.render error', e);
        }
    }

    return { render };
})();
