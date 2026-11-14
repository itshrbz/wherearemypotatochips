# Instalación

Todo está pensado para Windows. Hazlo una vez por computadora.

## 1. Todos

1. **Git** y, si prefieres algo visual, **GitHub Desktop**.
2. **Unity Hub** con el editor **6000.5.5f1**. Abre el proyecto desde la carpeta `Where_are_my_potato_chips`.
3. **Unity CLI**, en PowerShell:

   ```powershell
   winget install Unity.CLI
   unity --version
   ```

   Si `unity` no se reconoce, cierra y vuelve a abrir PowerShell.

4. Con el proyecto abierto en Unity, comprueba que la IA podrá controlar el editor. El paquete `com.unity.pipeline` ya está en el proyecto:

   ```powershell
   cd <ruta>\wherearemypotatochips\Where_are_my_potato_chips
   unity status
   ```

   Debe decir `ready`. Si no encuentra el editor, prueba `unity doctor` y revisa que Unity esté abierto con ese proyecto.

5. **Smart Merge para escenas.** En la raíz del repo ejecuta `git config --edit`, pega esto al final y guarda (ajusta la ruta si Unity está en otra carpeta):

   ```ini
   [merge]
       tool = unityyamlmerge
   [mergetool "unityyamlmerge"]
       trustExitCode = false
       cmd = 'C:/Program Files/Unity/Hub/Editor/6000.5.5f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"
   ```

   Cuando haya un conflicto en una escena, `git mergetool` lo intenta resolver solo.

## 2. Rafael y Grover: Claude Code

1. Instala Claude Code y entra con tu cuenta (Grover con su plan Pro). Las instrucciones oficiales están en la documentación de Claude Code.
2. Abre una terminal en la **raíz del repo** (donde está `CLAUDE.md`) y ejecuta `claude`.
3. La primera vez, Claude Code lee `.claude/settings.json` y te ofrece instalar los plugins del proyecto. Acepta. Si no aparece, instálalos a mano dentro de Claude Code:

   ```text
   /plugin marketplace add Unity-Technologies/unity-agent-plugin
   /plugin install unity@unity-agent-plugin
   /plugin marketplace add anthropics/claude-plugins-official
   /plugin install commit-commands@claude-plugins-official
   /plugin install code-review@claude-plugins-official
   /plugin install pr-review-toolkit@claude-plugins-official
   /plugin install ralph-loop@claude-plugins-official
   ```

4. Comprueba: escribe `/unity:` y deben aparecer las skills de Unity; escribe `/potato` y deben aparecer las cinco skills del equipo.
5. **Opcional, para que Claude entienda mejor el C#:** instala el SDK de .NET y luego

   ```powershell
   dotnet tool install --global csharp-ls
   ```

   y dentro de Claude Code `/plugin install csharp-lsp@claude-plugins-official`.

### Qué tan autónomo trabaja

`.claude/settings.json` ya deja que Claude edite archivos, use git (excepto push) y el Unity CLI sin preguntar cada vez. Siempre pregunta antes de `git push`, `reset`, `rebase` o borrar archivos, y tiene prohibido `push --force` y `reset --hard`. Con Shift+Tab cambias de modo dentro de la sesión. No uses el modo que salta todos los permisos: con este archivo no hace falta.

## 3. Alan: Codex

1. Instala Codex CLI y entra con tu cuenta de ChatGPT Plus:

   ```powershell
   npm install -g @openai/codex
   codex
   ```

2. Instala el plugin de Unity para Codex:

   ```powershell
   codex plugin marketplace add Unity-Technologies/unity-agent-plugin
   codex plugin add unity@unity-agent-plugin
   codex plugin list
   ```

3. Abre `codex` en la **raíz del repo**. Codex lee `AGENTS.md` automáticamente.
4. Para que trabaje sin pedir permiso a cada paso dentro del repo, elige el modo de aprobaciones automático dentro de la sesión (comando `/approvals`).

### Codex en local o en la nube

- **Local (recomendado):** con Unity abierto, Codex puede usar el Unity CLI para probar lo que hace.
- **Nube (chatgpt.com/codex, conectando el repo de GitHub):** sirve para tareas de solo código mientras Alan hace otra cosa, pero no puede abrir Unity. Esas tareas quedan "Para probar" hasta que Alan las abra en su editor.

## 4. Cristhian

Solo Git (o GitHub Desktop) y Unity. Para probar una rama:

```powershell
git fetch
git switch polish/look
```

Luego abre Unity, espera que importe y dale Play desde `00_Boot`. En GitHub Desktop es lo mismo con el selector de ramas.

## 5. Una vez, Rafael

Sube este setup al repo (tarea S1 en [RAFAEL.md](RAFAEL.md)).
