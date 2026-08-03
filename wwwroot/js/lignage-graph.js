// Fonction minimale pour rendre le graphe Highcharts depuis la config C#
let currentChart = null;

window.renderHighchartsGraph = function (containerId, config) {
    if (currentChart) {
        currentChart.destroy();
    }

    currentChart = Highcharts.chart(containerId, config);

    // Ajouter les flèches après le rendu
    setTimeout(function() {
        addArrowsToGraph(containerId);
    }, 500);
};

function addArrowsToGraph(containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;

    const svg = container.querySelector('svg');
    if (!svg) return;

    let defs = svg.querySelector('defs');
    if (!defs) {
        defs = document.createElementNS('http://www.w3.org/2000/svg', 'defs');
        svg.insertBefore(defs, svg.firstChild);
    }

    const oldMarker = defs.querySelector('#arrowhead');
    if (oldMarker) oldMarker.remove();

    const marker = document.createElementNS('http://www.w3.org/2000/svg', 'marker');
    marker.setAttribute('id', 'arrowhead');
    marker.setAttribute('markerWidth', '10');
    marker.setAttribute('markerHeight', '7');
    marker.setAttribute('refX', '9');
    marker.setAttribute('refY', '3.5');
    marker.setAttribute('orient', 'auto');
    marker.setAttribute('markerUnits', 'strokeWidth');

    const polygon = document.createElementNS('http://www.w3.org/2000/svg', 'polygon');
    polygon.setAttribute('points', '0 0, 10 3.5, 0 7');
    polygon.setAttribute('fill', '#666666');

    marker.appendChild(polygon);
    defs.appendChild(marker);

    const paths = svg.querySelectorAll('path.highcharts-link');
    paths.forEach(path => {
        path.setAttribute('marker-end', 'url(#arrowhead)');
    });
}
