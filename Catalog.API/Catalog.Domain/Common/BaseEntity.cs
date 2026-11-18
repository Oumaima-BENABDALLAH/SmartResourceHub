using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Common
{
    //BaseEntity : Classe de base partagée  pour toutes les entités du domaine

    //Fournit les propriétés fondamentales d'identification et d'audit (suivi temporel).
    public abstract class BaseEntity

    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow; // protected set L'Encapsulation empêches le code extérieur (comme les contrôleurs ou l'infrastructure) de modifier ces valeurs directement
                                                                             //Seule l'entité elle-même ou ses classes filles peuvent les altérer, ce qui protège tes règles métier
        public DateTime? UpdatedAt { get; protected set; }
        //Une méthode propre qui permet de mettre à jour le timestamp de modification uniquement quand une action métier le déclenche.
        protected void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;

    }
}
