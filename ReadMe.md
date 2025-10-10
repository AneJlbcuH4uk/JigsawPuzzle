# 🧩 Jigsaw Puzzle

A classic-style **Jigsaw Puzzle game** made with **Unity**.  
Players can assemble images by dragging and connecting puzzle pieces.

---

## 🎮 Overview

This is my first fully finished Unity project, developed independently from concept to final release.  
The goal was to create a complete, stable, and enjoyable game experience while learning core Unity systems such as texture manipulation, UI design, and sound management.

---

## 🕹️ Play / Download

You can try the game using one of the following builds:

-  [Windows Build (GitHub Release)](https://github.com/AneJlbcuH4uk/JigsawPuzzle/releases/download/1.0.0/Windows.Build.rar)
-  [Linux Build (GitHub Release)](https://github.com/AneJlbcuH4uk/JigsawPuzzle/releases/download/1.0.0/Linux.Build.rar)

---

## ✨ Features

- 🧠 **Puzzle Generation** – pieces of puzzles can be created from any image, that was added to game.  
- 🎨 **Multiple Collections** – categorized image packs, that can be easily added by developer.    
- 🌍 **Localization** – supports multiple languages.
- 💾 **Save System** – game has autosave feature + player can make saves himself.

---

## 🧩 Screenshots

<table>
  <tr>
    <td><img width="400" alt="image" src="https://github.com/user-attachments/assets/62dd2392-1cb8-4f84-9350-b924887ccc0d"/></td>
    <td><img width="400" alt="image" src="https://github.com/user-attachments/assets/5ad337be-78bc-4e7c-952a-e1adeb6535f0" /></td>
  </tr>
  <tr>
    <td><img width="400" alt="image" src="https://github.com/user-attachments/assets/c42e226a-e241-45ec-a46f-50fad2322c7f" /></td>
    <td><img width="400"  alt="image" src="https://github.com/user-attachments/assets/750395be-4c5c-42a9-9daf-a39999c94125" /></td>
  </tr>
</table>

---

## 📚 Development Highlights

During development, I focused on:
- Ensuring **smooth UI performance** when interacting with UI
- Ease of adding new image collections, and masks for image cutting

Personal discoveries and bigges bugfixes during development:
- Unity Colliders are recreated each frame if you`re moving them using transform. Which can tank performance drasticaly.
- Default SpriteMeshType when creating sprites are Tight. So if you want to add additional pixels to it using shaders be ready to see sprites that look cropped.
- Unity script execution order can have great impact on project.
- Making UI can take a lot of time, especially if you dont want to make it.
---


## 🧍‍♂️ Author

**Kovalchenko Andrii**  
Unity Developer | Mathematics Graduate  
📧 [Email]()
🌐 [GitHub](https://github.com/AneJlbcuH4uk)

---

## 📜 License

This project is published for educational and portfolio purposes.  
All custom code is free to explore and learn from.  
Artwork and audio assets belong to their respective owners.
