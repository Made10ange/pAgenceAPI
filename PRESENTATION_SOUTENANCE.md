# Support de présentation — Soutenance de Licence
**Conception et développement d'une application de gestion d'agence de voyage**

> Ce fichier contient le contenu slide par slide, prêt à copier-coller dans Genspark (ou tout générateur de PPT IA). Chaque section `##` = une diapositive. Le texte entre crochets [ ] est une instruction, pas du contenu à afficher.

---

## Slide 1 — Page de garde
- ISTAG — Institut Supérieur de Technologie Appliquée et de Gestion
- Licence Professionnelle en Informatique de Gestion
- **Conception et développement d'une application de gestion d'agence de voyage**
- Présenté par : YOUMBI FLORIDA
- Encadreur académique : M. BEKECK MARTIN ROLAND
- Encadreur professionnel : M. YAMSI CHRISTIAN
- Structure d'accueil : Fire Software, Yaoundé
- Année académique 2025–2026
[Design : fond sobre, logo école si disponible, photo/illustration de bus/voyage en arrière-plan discret]

---

## Slide 2 — Plan de la présentation
1. Présentation de la structure d'accueil
2. Déroulement du stage
3. Conception et réalisation du projet
4. Apports, limites et perspectives
5. Conclusion

---

## Slide 3 — Introduction
- Contexte : digitalisation croissante des services, gestion souvent encore manuelle (registres, Excel) dans les agences de transport interurbain
- Problématique : comment fiabiliser et automatiser la gestion des agences, véhicules, chauffeurs, voyages, billetterie et comptabilité ?
- Objectif : concevoir et développer une application web complète de gestion d'agence de voyage
- Méthodologie : modélisation UML, développement ASP.NET Core 8.0 (C#), MySQL

---

## PARTIE I — Présentation de la structure et déroulement du stage

## Slide 4 — Fire Software : présentation
- Entreprise créée en 2009, SARL, capital 150 000 000 FCFA
- Siège à Yaoundé, Carrefour Intendance
- Devise : « Rapidité – Efficacité »
- Domaines : formation, maintenance, développement logiciel, systèmes d'information géographique (SIG), audit SI, sites web

## Slide 5 — Missions, activités et environnement
- Missions : développement de solutions logicielles pour PME/PMI, déploiement réseaux d'entreprise, audit de systèmes d'information
- Ressources : salle de formation climatisée, salle de développement, équipe qualifiée + réseau d'experts certifiés
- Clients : PME/PMI, institutions de formation, organisations SIG
- Valeurs : respect, recherche de la perfection, exemplarité

## Slide 6 — Déroulement du stage
- Période : 12 janvier – 12 juin 2025
- Intégration progressive : découverte de l'existant, spécifications, développement, tests
- Encadrement académique et professionnel
[Frise chronologique simple sur cette slide]

---

## PARTIE II — Conception et réalisation du projet

## Slide 7 — Présentation générale du projet
- **AgenceV** : application de gestion d'agences de transport interurbain de voyageurs (bus/car)
- Gère : agences, véhicules, chauffeurs, voyages, passagers, billetterie, réservation en ligne avec paiement mobile money, comptabilité, personnel
- Deux applications distinctes qui communiquent entre elles : un back-office (frontend MVC) et une API REST

## Slide 8 — Architecture technique
- Deux projets ASP.NET Core 8.0 indépendants :
  - **Frontend MVC** (Razor) — vues, authentification, appels HTTP vers l'API
  - **API REST** — logique métier, accès aux données
- Communication HTTP/JSON entre les deux
- Base de données MySQL/MariaDB (36 tables)
[Insérer le schéma d'architecture en couches : Navigateur → Frontend MVC → API REST → MySQL]

## Slide 9 — Stack technologique
- **Backend** : ASP.NET Core 8.0 Web API, C#, pattern Repository, Dapper (SQL brut)
- **Frontend** : ASP.NET Core MVC, Razor
- **Base de données** : MySQL/MariaDB, procédures stockées pour la comptabilité
- **Autres** : QuestPDF (génération PDF), ClosedXML (export Excel), BCrypt (sécurité mots de passe), Swagger (documentation API)
- Modélisation : UML

## Slide 10 — Pourquoi Dapper plutôt qu'Entity Framework ?
- Dapper choisi comme accès aux données principal : performance et contrôle total du SQL
- Entity Framework Core présent dans l'architecture mais non exploité pour les requêtes métier
- Comptabilité : approche hybride avec procédures stockées MySQL pour centraliser la logique transactionnelle complexe

## Slide 11 — Organisation multi-agence
- Application conçue pour gérer **plusieurs agences** (succursales)
- Chaque agence dispose de ses propres chauffeurs, véhicules, voyages et passagers
- Isolation des données par agence à travers l'ensemble des modules

## Slide 12 — Modèle de données (vue d'ensemble)
- 36 tables organisées en 4 domaines logiques :
  1. **Transport** : agence, chauffeur, véhicule, voyage, passager, embarquement
  2. **Billetterie / Réservation** : billet, réservation, paiement
  3. **Comptabilité** : plan comptable, opérations, caisses, journée comptable
  4. **RH / Sécurité** : personnel, utilisateurs, groupes, privilèges
[Insérer un diagramme de classes simplifié si possible]

## Slide 13 — Module Voyages : la logique métier centrale
- Gestion des trajets programmés (départ, arrivée, horaires, véhicule, chauffeur)
- Numérotation journalière automatique des voyages
- **Détection automatique des conflits d'horaire** : impossible d'affecter un chauffeur ou véhicule déjà engagé sur un créneau qui se chevauche
- Suppression en cascade cohérente (affectations et embarquements liés)

## Slide 14 — Module Billetterie
- Cycle de vie complet du billet : vente, validation, report sur un autre voyage, changement de type, impression PDF
- Numéro de billet unique généré automatiquement (format BIL-AAAAMMJJ-XXXX)
- Distinction entre voyage prévu et voyage effectivement utilisé (permet le report)

## Slide 15 — Module Embarquements & Bagages/Colis
- Enregistrement des passagers présents à bord d'un voyage, avec numéro de siège
- Suivi des bagages et colis, avec génération de reçus, étiquettes et bordereaux de transport PDF
- Référence colis générée automatiquement, sur le même principe que la billetterie

## Slide 16 — Réservation en ligne & paiement mobile money
- Portail public accessible sans compte : recherche de voyages, réservation, paiement, récupération du billet
- Étapes : recherche → réservation → paiement mobile money → confirmation → billet électronique PDF → consultation ultérieure par référence
- Référence de réservation unique (format RES-AAAAMMJJ-XXXX)
- Intégration d'un service de paiement mobile money (gestion asynchrone des notifications de paiement)
[Insérer un diagramme de séquence simplifié du flux client]

## Slide 17 — Module Comptabilité et Caisse
- Plan comptable structuré selon le référentiel **OHADA/COBAC**, utilisé en Afrique francophone
- Écritures comptables en partie double, journée comptable (ouverture/fermeture)
- Gestion des caisses, transferts entre caisses, balance générale, brouillard de caisse
- **Écritures automatiques** : chaque vente de billet, bagage ou colis génère automatiquement son écriture comptable

## Slide 18 — Module Personnel (RH) & Sécurité
- Gestion du personnel, des postes et des fiches de paie
- Un chauffeur est automatiquement synchronisé comme employé RH
- Gestion des droits d'accès : utilisateurs, groupes, privilèges
- Historique des connexions et journal d'audit des actions utilisateur

## Slide 19 — Sécurité et sauvegarde
- Authentification par cookie avec gestion de session (8h, renouvellement glissant)
- Mots de passe protégés par hachage BCrypt
- Sauvegarde automatique de la base de données toutes les 24h (30 dernières sauvegardes conservées)

## Slide 20 — Démonstration
[Slide de transition — captures d'écran de l'application : tableau de bord, création d'un voyage, vente d'un billet, réservation publique, module comptabilité]
- Démonstration en direct de l'application

---

## Slide 21 — Apports du stage
- Maîtrise du développement d'une application web multi-couches en environnement professionnel
- Approfondissement des concepts d'architecture logicielle (pattern Repository, séparation frontend/API)
- Compréhension fonctionnelle de principes comptables (partie double, plan comptable OHADA/COBAC)
- Travail d'audit et de fiabilisation du code existant (suppression de modules obsolètes)

## Slide 22 — Limites du projet
- Authentification de l'API reposant sur un mécanisme simple (en-têtes), une évolution vers un système de jetons (JWT) est envisageable
- Absence de tests automatisés à ce stade
- Fonctionnalité de balance client en cours de finalisation

## Slide 23 — Perspectives d'évolution
- Migration de l'authentification API vers un schéma à jetons (JWT)
- Finalisation de la configuration du module de paiement mobile money en production
- Ajout de tests automatisés (unitaires et d'intégration)
- Finalisation de la balance client et nettoyage du code résiduel

## Slide 24 — Conclusion
- Rappel de l'objectif atteint : une application complète de gestion d'agence de voyage, de la réservation à la comptabilité
- Ce stage a permis une mise en pratique concrète des compétences acquises en formation
- Ouverture sur les perspectives d'évolution du projet

## Slide 25 — Remerciements & Questions
- Remerciements à l'encadreur académique, à l'encadreur professionnel, à Fire Software, au corps enseignant de l'ISTAG
- **Merci de votre attention — Place aux questions**
