# Whisper Transcriber (Local)

## 1. But du Projet

**Whisper Transcriber** est une application de bureau pour **Windows** conçue pour la transcription audio en texte.
Elle intègre les modèles d'intelligence artificielle **Whisper** d'OpenAI pour s'exécuter **100% localement**, sans connexion internet ni clé d'API.

L'objectif est de fournir un outil :

* **Gratuit**
* **Respectueux de la vie privée**
* **Facile d'accès**
  pour convertir la parole en texte, depuis un enregistrement en direct ou un fichier audio existant.

---

## 2. Fonctionnalités Principales

* **Transcription Locale** : traitement intégral sur la machine de l'utilisateur (aucune donnée envoyée à des serveurs externes).
* **Interface Graphique Intuitive** : utilisation sans ligne de commande.
* **Deux Modes de Transcription** :

  * **En Direct** : capture du microphone et transcription en temps réel.
  * **Depuis un Fichier** : prise en charge de nombreux formats (.mp3, .wav, .m4a, .flac, .ogg).
* **Prise en charge Multi-langues** :

  * Détection automatique
  * Possibilité de forcer une langue spécifique (Français, Anglais, Espagnol, etc.)
* **Sélection du Modèle Whisper** : choix entre `tiny`, `base`, `small`, `medium` pour équilibrer vitesse et précision.

> **Note :** L'export en formats comme SRT ou JSON n'est pas encore disponible.

---

## 3. Installation

### 1. Cloner le dépôt

```bash
git clone https://github.com/jungable/whisper-transcriber.git
```

### 2. Ouvrir la solution

* Ouvrir `Whisper Transcriber.sln` avec **Visual Studio 2022**
* Les dépendances **NuGet** (`Whisper.Net`, `NAudio`) seront restaurées automatiquement.

### 3. Télécharger les modèles Whisper

* Rendez-vous sur la page officielle des modèles : **whisper.cpp** sur [Hugging Face](https://huggingface.co/ggerganov/whisper.cpp).
* Téléchargez les fichiers `.bin` nécessaires (ex : `ggml-base.bin`, `ggml-small.bin`).

### 4. Ajouter les modèles au projet

* Glisser-déposer les fichiers `.bin` dans l'Explorateur de solutions (racine du projet).
* Dans les **Propriétés**, définir `Copier dans le répertoire de sortie` → **Copier si plus récent**.

### 5. Compiler

* Appuyer sur **F5** ou aller dans **Générer → Générer la solution**.

---

## 4. Usage

### Configuration

* Choisir le **modèle Whisper** dans la section *Paramètres*.
* Sélectionner la langue ou laisser en **Auto-détection**.

### Lancer la transcription

* **En Direct** :

  1. Cliquer sur *Démarrer la transcription en direct*.
  2. Parler dans le microphone.
  3. Cliquer sur *Arrêter* pour terminer.
* **Depuis un Fichier** :

  1. Cliquer sur *Sélectionner un fichier audio...*.
  2. Choisir le fichier.
  3. Cliquer sur *Transcrire le fichier sélectionné*.

### Résultat

* Le texte transcrit s’affiche dans la zone principale.

---

## 5. Exigences Système

* **OS** : Windows 10/11 (64-bit)
* **Framework** : .NET 8 Desktop Runtime
* **Langage** : C# 12
* **Bibliothèques** :

  * [Whisper.Net](https://github.com/sandrohanea/whisper.net) : wrapper .NET pour whisper.cpp
  * [NAudio](https://github.com/naudio/NAudio) : capture audio et conversion
* **Matériel** :

  * Optimisé pour **CPU** (pas besoin de GPU)
  * Les performances dépendent du processeur.

---

## 6. Licence

Distribué sous **Licence MIT**. Voir [LICENSE.txt](LICENSE.txt) pour plus de détails.

---

## 7. Contribuer

### Signaler un bug ou proposer une fonctionnalité

* Ouvrir une *Issue* sur GitHub avec description détaillée.

### Soumettre des modifications (Pull Request)

1. **Fork** du dépôt.
2. Créer une branche :

   ```bash
   git checkout -b feature/ma-super-fonctionnalite
   ```
3. Faire les modifications.
4. Committer :

   ```bash
   git commit -m "Ajout de ma super fonctionnalité"
   ```
5. Pousser la branche :

   ```bash
   git push origin feature/ma-super-fonctionnalite
   ```
6. Ouvrir une **Pull Request** vers le dépôt principal.

---

### Auteur

Projet développé par **jungable**.

