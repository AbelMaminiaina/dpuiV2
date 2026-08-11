// Fonction pour rendre le graphe Highcharts depuis la config C#
let currentChart = null;

window.renderHighchartsGraph = function (containerId, config) {
    if (currentChart) {
        currentChart.destroy();
    }

    // Debug: afficher les données
    console.log('=== GRAPH DEBUG ===');
    console.log('Container:', containerId);
    console.log('Links:', config.series[0].data);
    console.log('Links count:', config.series[0].data.length);
    console.log('Nodes:', config.series[0].nodes);
    console.log('Nodes count:', config.series[0].nodes.length);

    // Si pas de liens, afficher un message
    if (config.series[0].data.length === 0) {
        console.warn('WARNING: No links to display!');
        document.getElementById(containerId).innerHTML =
            '<div style="padding:20px;text-align:center;color:#999;">Aucun lien à afficher</div>';
        return;
    }

    try {
        currentChart = Highcharts.chart(containerId, config);
        console.log('Chart created successfully');

        // Ajouter les flèches après le rendu complet
        setTimeout(function() {
            addArrowsToGraph(containerId);
        }, 1500);
    } catch (error) {
        console.error('Error creating chart:', error);
        document.getElementById(containerId).innerHTML =
            '<div style="padding:20px;color:red;">Erreur: ' + error.message + '</div>';
    }
};

function addArrowsToGraph(containerId) {
    if (!currentChart) {
        console.log('No chart found');
        return;
    }

    const svg = currentChart.container.querySelector('svg');
    if (!svg) {
        console.log('No SVG found');
        return;
    }

    // Créer le defs pour les marqueurs
    let defs = svg.querySelector('defs');
    if (!defs) {
        defs = document.createElementNS('http://www.w3.org/2000/svg', 'defs');
        svg.insertBefore(defs, svg.firstChild);
    }

    // Supprimer l'ancien marqueur s'il existe
    const oldMarker = defs.querySelector('#arrowhead');
    if (oldMarker) oldMarker.remove();

    // Créer le marqueur flèche
    const marker = document.createElementNS('http://www.w3.org/2000/svg', 'marker');
    marker.setAttribute('id', 'arrowhead');
    marker.setAttribute('markerWidth', '12');
    marker.setAttribute('markerHeight', '10');
    marker.setAttribute('refX', '10');
    marker.setAttribute('refY', '5');
    marker.setAttribute('orient', 'auto');
    marker.setAttribute('markerUnits', 'userSpaceOnUse');

    const polygon = document.createElementNS('http://www.w3.org/2000/svg', 'polygon');
    polygon.setAttribute('points', '0 0, 12 5, 0 10');
    polygon.setAttribute('fill', '#555');

    marker.appendChild(polygon);
    defs.appendChild(marker);

    // Trouver tous les chemins (links) dans le graphique
    const allPaths = svg.querySelectorAll('path');
    let linkCount = 0;

    console.log('Total paths found:', allPaths.length);

    allPaths.forEach((path, index) => {
        const d = path.getAttribute('d');
        const stroke = path.getAttribute('stroke');
        const fill = path.getAttribute('fill');
        const className = path.getAttribute('class') || '';

        // Les liens sont des paths avec M et L (lignes droites), sans fill ou fill=none
        if (d && d.includes('M') && d.includes('L') &&
            (fill === 'none' || fill === null || fill === '') &&
            !d.includes('A') && !d.includes('C') && !d.includes('Z')) {

            path.setAttribute('marker-end', 'url(#arrowhead)');
            path.setAttribute('stroke', '#555');
            path.setAttribute('stroke-width', '2');
            linkCount++;
            console.log('Link path #' + linkCount + ':', d.substring(0, 50) + '...');
        }
    });

    console.log('Total arrows added:', linkCount);
}

// Fonction pour afficher un graphe simple si Highcharts ne fonctionne pas
window.renderSimpleGraph = function(containerId, nodes, links) {
    const container = document.getElementById(containerId);
    if (!container) return;

    let html = '<div style="padding:20px;">';
    html += '<h4>Noeuds (' + nodes.length + ')</h4><ul>';
    nodes.forEach(n => {
        html += '<li style="color:' + n.color + '">' + n.id + '</li>';
    });
    html += '</ul>';

    html += '<h4>Liens (' + links.length + ')</h4><ul>';
    links.forEach(l => {
        html += '<li>' + l.from + ' → ' + l.to + '</li>';
    });
    html += '</ul></div>';

    container.innerHTML = html;
};
