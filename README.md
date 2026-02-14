Rapport - Exercice 8 : Saisie des notes (CSV)
Implémenter l'export et l'import de notes via CSV, pour la Scolarité, en respectant la Clean Architecture.

- Mise en place de NoteCsvDto pour cartographier les lignes du fichier CSV. La propriété Note y est définie comme nullable pour gérer techniquement
les cases vides (absence ou retrait de note).
- Ajout de la méthode FindEtudiantsPourUeAsync dans le Repository étudiant. Pour récupérer les inscrits à une UE spécifique, en n'incluant que la note associée à cette même UE.
- Sécurisation des Use Cases d'import et d'export via la méthode IsAuthorized, qui bloque l'accès à tout utilisateur n'ayant pas le rôle Scolarite.
- Pour garantir l'intégrité de la base de données, l'importation (ImportNotesCsvUseCase) a été développée en deux passes :
      - 1 - Parcours du fichier pour vérifier l'existence des étudiants, des UEs, et la validité des notes (entre 0 et 20). La moindre anomalie lève une exception et annule tout.
      - 2 - Si le fichier est 100% valide, le code crée, met à jour ou supprime les notes (si la case du CSV a été vidée intentionnellement) avant d'appeler SaveChangesAsync().
- Création du SaisieNotesController. Ce contrôleur utilise la librairie CsvHelper pour gérer exclusivement le parsing et le formatage du fichier, délégant toute la logique
métier aux Use Cases.

La fonctionnalité a été intégralement testée via Swagger avec un compte Scolarité :

    Export : Génération correcte du fichier CSV pré-rempli pour une UE donnée.
    Import: Modification du fichier (ajouts, modifications et suppressions de notes) et mise à jour validée en base de données (Code 200).
    Import (Échec contrôlé) : Soumission d'un fichier avec une note hors barème (ex: 25). Le fichier a été entièrement rejeté (Code 400), 
    confirmant que la base de données reste intacte en cas d'erreur.


    pour se connecter : 
    {   "email": "stephanie.dertin@u-picardie.fr",   "password": "Miage2025#" }
