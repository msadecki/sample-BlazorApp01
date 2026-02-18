import Chart from 'https://cdn.jsdelivr.net/npm/chart.js/dist/chart.esm.min.js';

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
            console.error('exchangeChart.module: failed to parse points string', e);
            return [];
        }
    }
    if (typeof points === 'object') {
        if (typeof points.length === 'number') return Array.from(points);
        return Object.values(points);
    }
    return [];
}

export function render(points) {
    try {
        console.log('exchangeChart.module.render invoked with:', points);
        const arr = toArray(points);
        console.log('parsed array:', arr);

        const canvas = document.getElementById('exchangeChart');
        if (!canvas) {
            console.error('exchangeChart.module: canvas element with id "exchangeChart" not found');
            return;
        }

        const ctx = canvas.getContext('2d');
        if (!ctx) {
            console.error('exchangeChart.module: unable to get 2d context from canvas');
            return;
        }

        // Normalize incoming points: parse dates and numeric rates, drop invalid entries
        const normalized = arr.map(p => {
            const dateVal = p?.date;
            const parsed = new Date(dateVal);
            const label = (parsed && !isNaN(parsed.getTime())) ? parsed.toLocaleString() : (dateVal ?? '');
            const rate = Number(p?.rate);
            return { label, rate };
        }).filter(pt => typeof pt.label === 'string' && pt.label.length > 0 && Number.isFinite(pt.rate));

        if (normalized.length === 0) {
            console.warn('exchangeChart.module: no valid data points to render', arr);
            return;
        }

        console.log('normalized points:', normalized);

        const labels = normalized.map(p => p.label);
        const data = normalized.map(p => p.rate);

        if (chart) {
            chart.data.labels = labels;
            chart.data.datasets[0].data = data;
            // adjust y-axis range based on data
            const minVal = Math.min(...data);
            const maxVal = Math.max(...data);
            const span = Math.max((maxVal - minVal) * 0.1, 0.0001);
            chart.options.scales.y.min = Math.floor((minVal - span) * 100000) / 100000;
            chart.options.scales.y.max = Math.ceil((maxVal + span) * 100000) / 100000;
            console.log('exchangeChart.module: updating chart', { minVal, maxVal, labelsCount: labels.length, dataCount: data.length });
            chart.update();
            return;
        }

        // Chart imported as ESM at the top; create chart immediately
        createChart(ctx, labels, data);
    }
    catch (e) {
        console.error('exchangeChart.module.render error', e);
    }
}

function createChart(ctx, labels, data) {
    const minVal = Math.min(...data);
    const maxVal = Math.max(...data);
    const span = Math.max((maxVal - minVal) * 0.1, 0.0001);

    console.log('exchangeChart.module: creating chart', { minVal, maxVal, labelsCount: labels.length, dataCount: data.length });

    chart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'EUR → USD',
                data: data,
                parsing: false,
                borderColor: 'rgba(75, 192, 192, 1)',
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                tension: 0.2,
                pointRadius: 2,
                borderWidth: 3,
                fill: false
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: { type: 'category', display: true },
                y: {
                    display: true,
                    min: Math.floor((minVal - span) * 100000) / 100000,
                    max: Math.ceil((maxVal + span) * 100000) / 100000
                }
            }
        }
    });
    console.log('exchangeChart.module: chart created', chart);
}
