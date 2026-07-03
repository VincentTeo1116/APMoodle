let moduleChart = null;
let trendChart = null;
let chartInitialized = false;
let resizeTimeout = null;

document.addEventListener('DOMContentLoaded', function() {
    initializeAllCharts();
    
    // Listen for custom sidebar toggle event
    document.addEventListener('sidebarToggled', function(event) {
        console.log('Sidebar toggled, expanded:', event.detail.expanded);
        
        // Force reinitialization after sidebar animation completes
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(function() {
            forceReinitializeCharts();
        }, 350);
    });
    
    // Also listen for clicks on menu-toggle directly (backup)
    const menuToggle = document.getElementById('menuToggle');
    if (menuToggle) {
        menuToggle.addEventListener('click', function() {
            // Small delay to let CSS transition start
            setTimeout(function() {
                forceReinitializeCharts();
            }, 50);
        });
    }
    
    // Listen for window resize
    window.addEventListener('resize', function() {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(function() {
            if (moduleChart && trendChart) {
                moduleChart.resize();
                trendChart.resize();
            }
        }, 150);
    });
    
    // Listen for sidebar transition end (backup)
    const expandableMenu = document.getElementById('expandableMenu');
    if (expandableMenu) {
        expandableMenu.addEventListener('transitionend', function() {
            // Only reinitialize if charts might be broken
            if (moduleChart && !isChartVisible(moduleChart)) {
                forceReinitializeCharts();
            }
        });
    }
    
    // Listen for page visibility changes
    document.addEventListener('visibilitychange', function() {
        if (!document.hidden) {
            setTimeout(function() {
                if (moduleChart && trendChart) {
                    moduleChart.update();
                    trendChart.update();
                }
            }, 100);
        }
    });
});

// Force complete reinitialization of charts
function forceReinitializeCharts() {
    console.log('Force reinitializing charts...');
    
    // Check if charts exist and need reinitialization
    const moduleCanvas = document.getElementById('moduleChart');
    const trendCanvas = document.getElementById('trendChart');
    
    if (!moduleCanvas || !trendCanvas) return;
    
    // Destroy existing charts completely
    if (moduleChart) {
        try {
            moduleChart.destroy();
        } catch(e) {
            console.warn('Error destroying module chart:', e);
        }
        moduleChart = null;
    }
    
    if (trendChart) {
        try {
            trendChart.destroy();
        } catch(e) {
            console.warn('Error destroying trend chart:', e);
        }
        trendChart = null;
    }
    
    // Clear canvas contexts
    const moduleCtx = moduleCanvas.getContext('2d');
    const trendCtx = trendCanvas.getContext('2d');
    if (moduleCtx) moduleCtx.clearRect(0, 0, moduleCanvas.width, moduleCanvas.height);
    if (trendCtx) trendCtx.clearRect(0, 0, trendCanvas.width, trendCanvas.height);
    
    // Small delay to ensure DOM is ready
    setTimeout(() => {
        // Reinitialize with fresh instances
        initializeModuleChart();
        initializeTrendChart();
        initializeScoreCircles();
    }, 50);
}

// Helper function to check if chart is visible/rendered properly
function isChartVisible(chart) {
    if (!chart || !chart.canvas) return false;
    const rect = chart.canvas.getBoundingClientRect();
    return rect.width > 0 && rect.height > 0;
}

function initializeAllCharts() {
    initializeModuleChart();
    initializeTrendChart();
    initializeScoreCircles();
    chartInitialized = true;
}

function initializeModuleChart() {
    const canvas = document.getElementById('moduleChart');
    if (!canvas) {
        console.log('Module chart canvas not found');
        return;
    }

    const labels = window.moduleData?.labels || [];
    const values = window.moduleData?.values || [];
    
    console.log('Module chart data:', { labels, values });
    
    if (!labels.length || !values.length || values.every(v => v === 0)) {
        const container = canvas.closest('.card-body');
        if (container) {
            canvas.style.display = 'none';
            let noDataMsg = container.querySelector('.no-data-message');
            if (!noDataMsg) {
                noDataMsg = document.createElement('div');
                noDataMsg.className = 'no-data-message';
                noDataMsg.innerHTML = '<i class="fas fa-chart-pie"></i><p>No module data available</p>';
                container.appendChild(noDataMsg);
            }
            noDataMsg.style.display = 'flex';
        }
        return;
    }

    // Show canvas and hide no-data message
    canvas.style.display = 'block';
    const container = canvas.closest('.card-body');
    if (container) {
        const noDataMsg = container.querySelector('.no-data-message');
        if (noDataMsg) noDataMsg.style.display = 'none';
    }

    // Force canvas to have proper dimensions before chart creation
    canvas.style.width = '100%';
    canvas.style.height = 'auto';
    canvas.style.minHeight = '280px';
    
    // Force a reflow to get correct dimensions
    void canvas.offsetHeight;
    
    const total = values.reduce((a, b) => a + b, 0);
    const ctx = canvas.getContext('2d');
    const colors = generateColors(labels.length);
    
    // Create new chart
    try {
        moduleChart = new Chart(ctx, {
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
                cutout: '65%',
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const label = context.label || '';
                                const value = context.raw || 0;
                                const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                                return `${label}: ${value} (${percentage}%)`;
                            }
                        },
                        backgroundColor: '#1f2937',
                        titleColor: '#ffffff',
                        bodyColor: '#9ca3af',
                        padding: 10,
                        cornerRadius: 8
                    }
                }
            }
        });
        
        // Generate custom legend
        generateLegend(labels, values, colors, total);
    } catch (error) {
        console.error('Error creating module chart:', error);
    }
}

function generateLegend(labels, values, colors, total) {
    const legendContainer = document.getElementById('moduleLegend');
    if (!legendContainer) return;
    
    let legendHtml = '';
    labels.forEach((label, index) => {
        const percentage = total > 0 ? ((values[index] / total) * 100).toFixed(1) : 0;
        legendHtml += `
            <div class="legend-item">
                <span class="legend-color" style="background-color: ${colors[index % colors.length]}"></span>
                <span class="legend-label">${escapeHtml(label)}: </span>
                <span class="legend-value">${values[index]} (${percentage}%)</span>
            </div>
        `;
    });
    legendContainer.innerHTML = legendHtml;
}

function initializeTrendChart() {
    const canvas = document.getElementById('trendChart');
    if (!canvas) {
        console.log('Trend chart canvas not found');
        return;
    }

    const labels = window.trendData?.labels || [];
    const values = window.trendData?.values || [];
    
    console.log('Trend chart data:', { labels, values });
    
    if (!labels.length || !values.length || values.every(v => v === 0)) {
        canvas.style.display = 'none';
        const container = canvas.closest('.card-body');
        if (container) {
            let noDataMsg = container.querySelector('.no-data-message-trend');
            if (!noDataMsg) {
                noDataMsg = document.createElement('div');
                noDataMsg.className = 'no-data-message no-data-message-trend';
                noDataMsg.innerHTML = '<i class="fas fa-chart-line"></i><p>No trend data available</p>';
                container.appendChild(noDataMsg);
            }
            noDataMsg.style.display = 'flex';
        }
        return;
    }
    
    canvas.style.display = 'block';
    const container = canvas.closest('.card-body');
    if (container) {
        const noDataMsg = container.querySelector('.no-data-message-trend');
        if (noDataMsg) noDataMsg.style.display = 'none';
    }
    
    // Force proper dimensions
    canvas.style.width = '100%';
    canvas.style.height = 'auto';
    canvas.style.minHeight = '260px';
    void canvas.offsetHeight;
    
    const ctx = canvas.getContext('2d');
    
    // Create new chart
    try {
        trendChart = new Chart(ctx, {
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
                    pointBorderColor: '#ffffff',
                    pointBorderWidth: 2,
                    pointRadius: 5,
                    pointHoverRadius: 8,
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: '#1f2937',
                        titleColor: '#ffffff',
                        bodyColor: '#9ca3af',
                        padding: 10,
                        cornerRadius: 8,
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
                            color: '#e5e7eb',
                            drawBorder: false
                        },
                        ticks: {
                            stepSize: 1,
                            precision: 0
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        }
                    }
                }
            }
        });
    } catch (error) {
        console.error('Error creating trend chart:', error);
    }
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
        return { start: '#10b981', end: '#059669' };
    } else if (score >= 60) {
        return { start: '#f59e0b', end: '#d97706' };
    } else if (score >= 40) {
        return { start: '#f97316', end: '#ea580c' };
    } else {
        return { start: '#ef4444', end: '#dc2626' };
    }
}

function generateColors(count) {
    const baseColors = [
        '#4f46e5', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6',
        '#ec4899', '#06b6d4', '#84cc16', '#f97316', '#6366f1',
        '#14b8a6', '#d946ef', '#f43f5e', '#0ea5e9', '#eab308'
    ];
    
    if (count <= baseColors.length) {
        return baseColors.slice(0, count);
    }
    
    const generated = [...baseColors];
    for (let i = baseColors.length; i < count; i++) {
        const hue = (i * 137) % 360;
        generated.push(`hsl(${hue}, 70%, 55%)`);
    }
    return generated;
}

function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/[&<>]/g, function(m) {
        if (m === '&') return '&amp;';
        if (m === '<') return '&lt;';
        if (m === '>') return '&gt;';
        return m;
    });
}