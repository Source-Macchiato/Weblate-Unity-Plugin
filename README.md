# Weblate for Unity

Weblate for Unity is a plugin inspired by Crowdin's plugin that allows Unity programmers to pull and push translations directly between the Unity localization package and the Weblate project.

## Installation

1. On your Unity project click on *Edit* > *Project Settings* and select *Package Manager*
2. Create a new scoped registry with the following data:
   - Name: `NPM`
   - URL: `https://registry.npmjs.org`
   - Scope(s): `com.sourcemacchiato.weblate`
3. Click on *Apply*
4. Open the *Package Manager* and select *My Registries*
5. Select *Weblate* and click on the *Install* button

## Configuration

Before use the plugin you have to open *Tools* > *Weblate* > *Settings* and fill these values.

### Host

Enter the url of the host. At the time the plugin doesn't correct it so be careful to add `http://` or `https://` and to not add a `/` at the end of the url. For exemple `https://hosted.weblate.org`.

### Token

You can use either a user token (starting with `wlu_`) or a project token (starting with `wlp_`), but for security reasons we recommend using a project token.

### Slug

Enter the slug of your project.

### File Type

In this dropdown you can select between `CSV`, `Json` and `PO` formats.

## Usage

At the time only `Pull string translations from Weblate` is available. `Push string translations`, `Pull asset translations` and `Push asset translations` will come in future updates.

### Pull string translations

1. Click on `Tools` > `Weblate` > `Pull string translations`.
2. Choose between `All tables` or `Selected tables`. If you selected the second option select tables you want to pull.
3. Click on `Pull string translations from Weblate`.