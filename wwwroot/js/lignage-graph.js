window.lignageGraph = {
    chart: null,

    render: function (containerId, data) {
        // data = { noeudActuel, successeurs, predecesseurs, historique }

        const links = [];
        const nodes = [];
        const nodeSet = new Set();

        // Ajouter le noeud actuel
        const currentNode = data.noeudActuel;
        const currentNodeTrunc = this.truncate(currentNode, 20);
        nodeSet.add(currentNodeTrunc);
        nodes.push({
            id: currentNodeTrunc,
            color: '#4CAF50',
            marker: { radius: 25 }
        });

        // Ajouter les predecesseurs (fleche vers le noeud actuel)
        data.predecesseurs.forEach(p => {
            const pTrunc = this.truncate(p.noeud, 20);
            if (!nodeSet.has(pTrunc)) {
                nodeSet.add(pTrunc);
                nodes.push({
                    id: pTrunc,
                    color: '#2196F3',
                    marker: { radius: 15 }
                });
            }
            links.push([pTrunc, currentNodeTrunc]);
        });

        // Ajouter les successeurs (fleche depuis le noeud actuel)
        data.successeurs.forEach(s => {
            const sTrunc = this.truncate(s.noeud, 20);
            if (!nodeSet.has(sTrunc)) {
                nodeSet.add(sTrunc);
                nodes.push({
                    id: sTrunc,
                    color: '#FF9800',
                    marker: { radius: 15 }
                });
            }
            links.push([currentNodeTrunc, sTrunc]);
        });

        // Ajouter l'historique
        for (let i = 0; i < data.historique.length - 1; i++) {
            const fromTrunc = this.truncate(data.historique[i].noeud, 20);
            const toTrunc = this.truncate(data.historique[i + 1].noeud, 20);

            if (!nodeSet.has(fromTrunc)) {
                nodeSet.add(fromTrunc);
                nodes.push({
                    id: fromTrunc,
                    color: '#9E9E9E',
                    marker: { radius: 12 }
                });
            }
            if (!nodeSet.has(toTrunc)) {
                nodeSet.add(toTrunc);
                nodes.push({
                    id: toTrunc,
                    color: '#9E9E9E',
                    marker: { radius: 12 }
                });
            }

            // Verifier si le lien existe deja
            const linkExists = links.some(l => l[0] === fromTrunc && l[1] === toTrunc);
            if (!linkExists) {
                links.push([fromTrunc, toTrunc]);
            }
        }

        // Detruire le graphe existant
        if (this.chart) {
            this.chart.destroy();
        }

        // Creer le graphe avec fleches
        this.chart = Highcharts.chart(containerId, {
            chart: {
                type: 'networkgraph',
                height: 450
            },
            title: {
                text: 'Graphe de Lignage'
            },
            subtitle: {
                text: 'Vert: Actuel | Bleu: Predecesseurs | Orange: Successeurs | Gris: Historique'
            },
            plotOptions: {
                networkgraph: {
                    keys: ['from', 'to'],
                    layoutAlgorithm: {
                        enableSimulation: true,
                        linkLength: 120,
                        gravitationalConstant: 0.02
                    },
                    link: {
                        width: 2,
                        color: '#666666'
                    }
                }
            },
            series: [{
                marker: {
                    radius: 15
                },
                dataLabels: {
                    enabled: true,
                    linkFormat: '',
                    style: {
                        fontSize: '11px',
                        fontWeight: 'normal'
                    },
                    y: -10
                },
                link: {
                    width: 2
                },
                data: links,
                nodes: nodes
            }]
        });

        // Ajouter les fleches avec SVG apres le rendu
        setTimeout(() => {
            this.addArrows(containerId);
        }, 500);
    },

    addArrows: function(containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const svg = container.querySelector('svg');
        if (!svg) return;

        // Creer le marker pour les fleches
        let defs = svg.querySelector('defs');
        if (!defs) {
            defs = document.createElementNS('http://www.w3.org/2000/svg', 'defs');
            svg.insertBefore(defs, svg.firstChild);
        }

        // Supprimer les anciens markers
        const oldMarker = defs.querySelector('#arrowhead');
        if (oldMarker) oldMarker.remove();

        // Creer le marker fleche
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

        // Appliquer les fleches aux liens
        const paths = svg.querySelectorAll('path.highcharts-link');
        paths.forEach(path => {
            path.setAttribute('marker-end', 'url(#arrowhead)');
        });
    },

    truncate: function (text, max) {
        if (!text || text.length <= max) return text;
        return text.substring(0, max) + '...';
    }
};
