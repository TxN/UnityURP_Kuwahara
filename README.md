# UnityURP_Kuwahara
Unity URP RenderFeature that implements Kuwahara (oil painting) Post Processing Effect


![alt text](https://github.com/TxN/UnityURP_Kuwahara/blob/main/Samples~/simple.jpg?raw=true)
*An example of Kuwahara effect in game*

I take no credit for shaders used in this effect, these shaders were made by Acerola ( https://github.com/GarrettGunnell/Post-Processing/ )

## Instructions
- Open your project manifest file (`MyProject/Packages/manifest.json`).
- Add `"com.mehozavr.kuwaharaURP": "https://github.com/TxN/UnityURP_Kuwahara.git"` to the `dependencies` list.
- Open or focus on Unity Editor to resolve packages.
- Add "Kuwahara Effect Render Feature" to your Render Pipeline Settings
- Add "Custom > Kuwahara" to your Post Processing Volume

## Requirements
- Unity 2021.3.0 or higher.

## Examples
### No effect
![alt text](https://github.com/TxN/UnityURP_Kuwahara/blob/main/Samples~/no_effect.jpg?raw=true)
*Reference image without effect applied*

### Simple
![alt text](https://github.com/TxN/UnityURP_Kuwahara/blob/main/Samples~/simple.jpg?raw=true)
*An example of Simple Kuwahara effect in game*

### Generalized
![alt text](https://github.com/TxN/UnityURP_Kuwahara/blob/main/Samples~/generalized.jpg?raw=true)
*An example of Generalized Kuwahara effect in game*

### Anisotropic
![alt text](https://github.com/TxN/UnityURP_Kuwahara/blob/main/Samples~/anisotropic.jpg?raw=true)
*An example of Anisotropic Kuwahara effect in game*

### Effect Settings
![alt text](https://github.com/TxN/UnityURP_Kuwahara/blob/main/Samples~/kwh_settings.PNG?raw=true)
*Effect settings menu screenshot*
