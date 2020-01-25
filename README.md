# TSystem

TSystem is a modular rhythm game library for Unity.

## Features

* All fundamental features for rhythm game
* Note, Parser, Judger, Input and related subscripts are ready for Import-and-Play
* Developer-customizable ingame manager called **Basis**
* User-customizable data file called **Mode**

### Basis

Basis is the "Skeleton" of the game. Basis works as ingame manager which controls time, notes and other managable things.

To write your own Basis, just inherit ```IngameBasis``` to your class.  

You should write overrided methods for:
* ```GetNoteTemplate``` (Returns a ```GameObject``` depending on the note data)
* ```GetNoteImage``` (Returns a ```Sprite``` depending on the note data)
* ```CreeateNote``` (Creates a ```Note``` from the note data)
* ```AfterNoteLoading``` (Called after the notes are fully loaded.)
* ```AddScore``` (Calculates and adds the score from given note data and ```JudgeType```)

### Mode

Mode is the "Skin" of the game. Mode file is NOT a script; it is a JSON-format data file.  
This means that you don't need to modify the binary to change the values from Mode.

Mode includes:
* Name
* Description
* Basis index to use
* Line informations
* Judging informations
* Path informations (Math expression and Bezier points are available for path definition)

## Installing TSystem

Go to Releases, and download the latest package file. Then, in your Unity project, import the downloaded package file.  
To get access to TSystem classes and functions in your code, add namespace declaration: ```using TSystem;```

## More informations

See [wiki](https://github.com/thiEFcat/TSystem/wiki, "Wiki Page") for Concept description and Documentation.
