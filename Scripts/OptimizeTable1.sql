-- Optimisation: Ajouter des colonnes de hash indexables
USE LignageDb2;
GO

-- Ajouter les colonnes de hash (VARBINARY(32) pour SHA2_256)
ALTER TABLE table1 ADD noeuds1_hash AS HASHBYTES('SHA2_256', noeuds1) PERSISTED;
ALTER TABLE table1 ADD noeuds1lie_hash AS HASHBYTES('SHA2_256', noeuds1lie) PERSISTED;
GO

-- Créer les index sur les colonnes de hash
CREATE INDEX IX_table1_noeuds1_hash ON table1 (noeuds1_hash);
CREATE INDEX IX_table1_noeuds1lie_hash ON table1 (noeuds1lie_hash);
GO

-- Index composite pour les recherches fréquentes
CREATE INDEX IX_table1_noeuds1_hash_edgdir ON table1 (noeuds1_hash, edg_dir);
CREATE INDEX IX_table1_noeuds1lie_hash_edgdir ON table1 (noeuds1lie_hash, edg_dir);
GO

PRINT 'Colonnes de hash et index créés avec succès!';
GO
