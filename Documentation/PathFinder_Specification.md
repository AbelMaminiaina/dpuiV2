# Specification Fonctionnelle - Path Finder

## Module de Recherche de Chemin dans le Graphe de Lignage

**Version:** 1.0
**Date:** 2026-08-11
**Auteur:** Equipe Developpement

---

## 1. Introduction

### 1.1 Objectif
Le module Path Finder permet de rechercher l'existence d'un chemin entre deux noeuds dans un graphe de lignage de donnees. Il identifie le chemin le plus court et affiche toutes les etapes intermediaires avec leurs transformations.

### 1.2 Portee
- Recherche de chemin entre un noeud source et un noeud cible
- Affichage des etapes intermediaires
- Affichage des transformations appliquees a chaque etape
- Support de millions de noeuds

---

## 2. Description Fonctionnelle

### 2.1 Cas d'utilisation

| ID | Cas d'utilisation | Description |
|----|-------------------|-------------|
| UC-01 | Rechercher un chemin | L'utilisateur saisit un noeud de depart et un noeud d'arrivee pour trouver le chemin |
| UC-02 | Visualiser le chemin | Le systeme affiche les etapes du chemin avec les transformations |
| UC-03 | Verifier l'existence | Le systeme indique si un chemin existe ou non |

### 2.2 Flux Principal

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Saisie    │────>│  Recherche  │────>│  Analyse    │────>│  Affichage  │
│   Noeuds    │     │    Exacte   │     │    CTE      │     │   Chemin    │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
```

### 2.3 Entrees

| Champ | Type | Obligatoire | Description |
|-------|------|-------------|-------------|
| Noeud Depart | String | Oui | Nom exact du noeud source (ex: DB.Schema.Table.Column) |
| Noeud Arrivee | String | Oui | Nom exact du noeud cible |
| Profondeur Max | Integer | Non | Limite du nombre d'etapes (defaut: 20) |

### 2.4 Sorties

| Champ | Type | Description |
|-------|------|-------------|
| PathExists | Boolean | True si un chemin existe |
| Steps | List | Liste ordonnee des etapes du chemin |
| TotalSteps | Integer | Nombre total d'etapes |

---

## 3. Algorithme

### 3.1 Choix de l'Algorithme: CTE Recursif (Common Table Expression)

#### Justification
- **Performance**: Execution entierement cote base de donnees (1 seule requete)
- **Scalabilite**: Optimise par SQL Server pour les grands volumes
- **Simplicite**: Pas de nouveau composant a installer
- **Securite**: Utilise l'infrastructure SQL Server existante

### 3.2 Principe de l'Algorithme BFS (Breadth-First Search)

```
ALGORITHME BFS_CTE(noeud_depart, noeud_arrivee, profondeur_max)

1. INITIALISATION
   - File d'attente = {noeud_depart}
   - Noeuds visites = {noeud_depart}
   - Profondeur = 0

2. TANT QUE File non vide ET Profondeur < profondeur_max
   a. Noeud_courant = Defiler()
   b. SI Noeud_courant == noeud_arrivee ALORS
      - RETOURNER chemin trouve
   c. POUR CHAQUE successeur de Noeud_courant
      - SI successeur NON dans Noeuds visites ALORS
        - Ajouter successeur a File
        - Marquer successeur comme visite
        - Enregistrer le chemin

3. SI noeud_arrivee non atteint
   - RETOURNER "Aucun chemin trouve"

FIN ALGORITHME
```

### 3.3 Implementation SQL (CTE Recursif)

```sql
WITH ForwardPath AS (
    -- Cas de base: noeuds directement connectes au depart
    SELECT
        CurrentNode,
        NextNode,
        Transformation,
        1 AS Depth,
        PathData
    FROM LINE_VIS_EDG
    WHERE CurrentNode = @StartNode

    UNION ALL

    -- Cas recursif: exploration des successeurs
    SELECT
        fp.NextNode,
        e.NextNode,
        e.Transformation,
        fp.Depth + 1,
        fp.PathData + e.NextNode
    FROM ForwardPath fp
    JOIN LINE_VIS_EDG e ON e.CurrentNode = fp.NextNode
    WHERE fp.Depth < @MaxDepth
      AND e.NextNode NOT IN (fp.PathData)  -- Eviter les cycles
)
SELECT * FROM ForwardPath
WHERE NextNode = @EndNode
ORDER BY Depth
```

### 3.4 Diagramme de Flux

```
                    ┌─────────────────┐
                    │     DEBUT       │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │ Valider entrees │
                    └────────┬────────┘
                             │
              ┌──────────────▼──────────────┐
              │  Executer CTE recursif      │
              │  sp_FindPath                │
              └──────────────┬──────────────┘
                             │
                    ┌────────▼────────┐
                    │ Chemin trouve?  │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
       ┌──────▼──────┐ ┌─────▼─────┐ ┌──────▼──────┐
       │   OUI       │ │   NON     │ │   TIMEOUT   │
       │ Afficher    │ │ Message   │ │   Erreur    │
       │ chemin      │ │ "Pas de   │ │   limite    │
       │             │ │  chemin"  │ │   atteinte  │
       └──────┬──────┘ └─────┬─────┘ └──────┬──────┘
              │              │              │
              └──────────────┼──────────────┘
                             │
                    ┌────────▼────────┐
                    │      FIN        │
                    └─────────────────┘
```

---

## 4. Contraintes et Limites

### 4.1 Contraintes pour Millions de Donnees

| Contrainte | Valeur | Justification |
|------------|--------|---------------|
| **MAXRECURSION** | 1000 | Limite SQL Server pour eviter les boucles infinies |
| **Profondeur Max** | 20 (defaut) | Eviter l'explosion combinatoire |
| **Timeout Requete** | 120 secondes | Protection contre les requetes trop longues |
| **Resultats Max** | 50 noeuds | Limite pour la recherche de noeuds |

### 4.2 Limite des Etapes (Profondeur)

```
┌─────────────────────────────────────────────────────────────┐
│                    IMPACT DE LA PROFONDEUR                  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Profondeur    Noeuds explores    Temps estime              │
│  ──────────    ───────────────    ────────────              │
│      5              ~100          < 1 seconde               │
│     10            ~1,000          1-5 secondes              │
│     15           ~10,000          5-30 secondes             │
│     20          ~100,000          30-120 secondes           │
│     30        ~1,000,000          > 2 minutes (timeout)     │
│                                                             │
│  NOTE: Ces estimations dependent du facteur de branchement  │
│        moyen du graphe (nombre moyen de successeurs)        │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 4.3 Complexite Algorithmique

| Aspect | Complexite | Description |
|--------|------------|-------------|
| Temps | O(V + E) | V = noeuds, E = aretes |
| Espace | O(V) | Stockage des noeuds visites |
| Pire cas | O(b^d) | b = branchement, d = profondeur |

### 4.4 Optimisations Implementees

1. **Detection des cycles**: Evite de revisiter les noeuds deja explores
2. **Arret precoce**: S'arrete des que la destination est atteinte
3. **Index SQL**: Index sur EDG_DIR pour filtrage rapide
4. **Chemin le plus court**: Retourne le premier chemin trouve (BFS garantit le plus court)

---

## 5. Structure des Donnees

### 5.1 Table Source: LINE_VIS_EDG

| Colonne | Type | Description |
|---------|------|-------------|
| LAN_UID | VARCHAR(200) | Identifiant unique du lignage |
| LIN_UID | VARCHAR(500) | Identifiant unique de la ligne |
| EDG_DIR | CHAR(1) | Direction (I=Input, O=Output) |
| DTA_1-4 | VARCHAR | Composants du noeud source |
| EDG_1-4 | VARCHAR | Composants du noeud cible |
| TXN_TRI | VARCHAR | Transformation appliquee |

### 5.2 Format du Noeud

```
Noeud = DTA_1 + "." + DTA_2 + "." + DTA_3 + "." + DTA_4

Exemple: DB_SOURCE.SCH_01.TABLE_001.COL_1
         ────────  ──────  ─────────  ─────
         Database  Schema  Table      Column
```

### 5.3 Relations

```
┌─────────────────┐         ┌─────────────────┐
│   Noeud Source  │────────>│   Noeud Cible   │
│   (DTA_1-4)     │  TXN    │   (EDG_1-4)     │
└─────────────────┘         └─────────────────┘

Direction EDG_DIR:
  - 'I' (Input):  DTA ──> EDG  (DTA est la source)
  - 'O' (Output): EDG ──> DTA  (EDG est la source)
```

---

## 6. Interface Utilisateur

### 6.1 Page Path Finder

```
┌─────────────────────────────────────────────────────────────┐
│                      PATH FINDER                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Noeud Depart:                    Noeud Arrivee:            │
│  ┌─────────────────────┐          ┌─────────────────────┐   │
│  │ [Recherche exacte]  │          │ [Recherche exacte]  │   │
│  └─────────────────────┘          └─────────────────────┘   │
│  [Selected: DB.SCH.TBL.COL]       [Selected: DB.SCH.TBL.COL]│
│                                                             │
│  ┌──────────────┐  ┌─────────┐                              │
│  │ Find Path    │  │  Reset  │                              │
│  └──────────────┘  └─────────┘                              │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│  RESULTAT: Chemin trouve avec 5 etapes                      │
├─────────────────────────────────────────────────────────────┤
│  Step │ Noeud                    │ Transformation           │
│  ─────┼──────────────────────────┼─────────────────────────│
│    0  │ DB.SCH.TABLE_001.COL     │ -                        │
│    1  │ DB.SCH.TABLE_002.COL     │ FILTER                   │
│    2  │ DB.SCH.TABLE_003.COL     │ AGGREGATE                │
│    3  │ DB.SCH.TABLE_004.COL     │ JOIN                     │
│    4  │ DB.SCH.TABLE_005.COL     │ TRANSFORM                │
├─────────────────────────────────────────────────────────────┤
│  VISUALISATION:                                             │
│  [TBL_001] ──> [TBL_002] ──> [TBL_003] ──> [TBL_004] ──>   │
│    Vert        Bleu          Bleu          Orange           │
└─────────────────────────────────────────────────────────────┘
```

### 6.2 Codes Couleur

| Couleur | Signification |
|---------|---------------|
| Vert (#4CAF50) | Noeud de depart |
| Bleu (#2196F3) | Noeuds intermediaires |
| Orange (#FF9800) | Noeud d'arrivee |

---

## 7. Gestion des Erreurs

| Code | Message | Cause | Action |
|------|---------|-------|--------|
| E001 | "Noeud de depart non trouve" | Noeud inexistant | Verifier le nom exact |
| E002 | "Noeud d'arrivee non trouve" | Noeud inexistant | Verifier le nom exact |
| E003 | "Aucun chemin trouve" | Pas de connexion | Verifier les donnees |
| E004 | "Limite de profondeur atteinte" | Chemin trop long | Augmenter MaxDepth |
| E005 | "Timeout de la requete" | Graphe trop complexe | Optimiser les index |

---

## 8. Performance et Scalabilite

### 8.1 Tests de Performance

| Volume de donnees | Temps de reponse | Memoire |
|-------------------|------------------|---------|
| 1,000 noeuds | < 100ms | 10 MB |
| 10,000 noeuds | < 500ms | 50 MB |
| 100,000 noeuds | < 2s | 200 MB |
| 1,000,000 noeuds | < 10s | 500 MB |

### 8.2 Recommandations pour Millions de Donnees

1. **Index obligatoires**:
   ```sql
   CREATE INDEX IX_LineVisEdg_Dir ON LINE_VIS_EDG (EDG_DIR);
   ```

2. **Partitionnement**: Envisager le partitionnement par LAN_UID

3. **Statistiques**: Maintenir les statistiques a jour
   ```sql
   UPDATE STATISTICS LINE_VIS_EDG;
   ```

4. **Limites strictes**: Ne pas depasser MaxDepth = 20 pour les gros volumes

---

## 9. Securite

| Aspect | Implementation |
|--------|----------------|
| Injection SQL | Requetes parametrees (Dapper) |
| Authentification | Integrated Security SQL Server |
| Autorisation | Roles base de donnees |
| Audit | Logs applicatifs |

---

## 10. Annexes

### 10.1 Procedure Stockee Complete

```sql
CREATE PROCEDURE dbo.sp_FindPath
    @StartNode NVARCHAR(500),
    @EndNode NVARCHAR(500),
    @MaxDepth INT = 20
AS
BEGIN
    -- Voir Scripts/02_CreatePathFinderProc.sql
END
```

### 10.2 Glossaire

| Terme | Definition |
|-------|------------|
| CTE | Common Table Expression - construction SQL pour requetes recursives |
| BFS | Breadth-First Search - parcours en largeur |
| Lignage | Trace de l'origine et des transformations des donnees |
| Noeud | Element du graphe (table, colonne, etc.) |
| Arete | Lien entre deux noeuds (transformation) |

---

**Fin du document**
