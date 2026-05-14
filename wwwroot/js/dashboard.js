// dashboard.js

document.addEventListener('DOMContentLoaded', function() {
    // Get data from server-side injected variables
    initializeModuleChart();
    initializeTrendChart();
    initializeScoreCircles();
});

function initializeModuleChart() {
    const canvas = document.getElementById('moduleChart');
    if (!canvas) return;

    // Get data from data attributes on canvas or from window object
    const labels = window.moduleChartLabels || [];
    const values = window.moduleChartValues || [];
    
    if (!labels.length) {
        const noDataDiv = canvas.parentElement?.querySelector('.no-data-message');
        if (noDataDiv) noDataDiv.style.display = 'flex';
        canvas.style.display = 'none';
        return;
    }

    const ctx = canvas.getContext('2d');
    
    // Generate colors
    const colors = generateColors(labels.length);
    
    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                backgroundColor: colors,
                borderWidth: 0,
                hoverOffset: 10
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            cutout: '60%',
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        boxWidth: 12,
                        padding: 15,
                        font: {
                            size: 11
                        },
                        generateLabels: function(chart) {
                            const data = chart.data;
                            if (data.labels.length && data.datasets.length) {
                                return data.labels.map((label, i) => {
                                    const value = data.datasets[0].data[i];
                                    const total = data.datasets[0].data.reduce((a, b) => a + b, 0);
                                    const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                                    return {
                                        text: `${label} (${percentage}%)`,
                                        fillStyle: data.datasets[0].backgroundColor[i],
                                        index: i
                                    };
                                });
                            }
                            return [];
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const label = context.label || '';
                            const value = context.raw || 0;
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                            return `${label}: ${value} (${percentage}%)`;
                        }
                    }
                }
            }
        }
    });
}

function initializeTrendChart() {
    const canvas = document.getElementById('trendChart');
    if (!canvas) return;

    const labels = window.trendChartLabels || [];
    const values = window.trendChartValues || [];
    
    if (!labels.length) {
        canvas.style.display = 'none';
        return;
    }

    const ctx = canvas.getContext('2d');
    
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Enrollments',
                data: values,
                borderColor: '#4f46e5',
                backgroundColor: 'rgba(79, 70, 229, 0.1)',
                borderWidth: 3,
                pointBackgroundColor: '#4f46e5',
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                pointRadius: 5,
                pointHoverRadius: 7,
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    position: 'top',
                    labels: {
                        usePointStyle: true,
                        boxWidth: 10
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            return `Enrollments: ${context.raw}`;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: '#e5e7eb'
                    },
                    title: {
                        display: true,
                        text: 'Number of Enrollments',
                        color: '#6b7280'
                    }
                },
                x: {
                    grid: {
                        display: false
                    },
                    title: {
                        display: true,
                        text: 'Month',
                        color: '#6b7280'
                    }
                }
            }
        }
    });
}

function initializeScoreCircles() {
    const scoreCircles = document.querySelectorAll('.score-circle');
    scoreCircles.forEach(circle => {
        const score = parseFloat(circle.getAttribute('data-score') || '0');
        const color = getScoreColor(score);
        circle.style.background = `linear-gradient(135deg, ${color.start}, ${color.end})`;
    });
}

function getScoreColor(score) {
    if (score >= 80) {
        return { start: '#10b981', end: '#059669' };  // Green
    } else if (score >= 60) {
        return { start: '#f59e0b', end: '#d97706' };  // Orange
    } else if (score >= 40) {
        return { start: '#f97316', end: '#ea580c' };  // Dark Orange
    } else {
        return { start: '#ef4444', end: '#dc2626' };  // Red
    }
}

function generateColors(count) {
    const colors = [
        '#4f46e5', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6',
        '#ec4899', '#06b6d4', '#84cc16', '#f97316', '#6366f1',
        '#14b8a6', '#d946ef', '#f43f5e', '#0ea5e9', '#eab308'
    ];
    
    if (count <= colors.length) {
        return colors.slice(0, count);
    }
    
    // Generate more colors if needed
    const generated = [...colors];
    for (let i = colors.length; i < count; i++) {
        const hue = (i * 137) % 360;
        generated.push(`hsl(${hue}, 70%, 55%)`);
    }
    return generated;
}