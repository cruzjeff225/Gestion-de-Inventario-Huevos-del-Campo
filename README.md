
# Guía Rápida para contribuir al repo🚀

👩‍💻 Acá te explico cómo colaborar de manera efectiva en este repo. Paso a paso a continuación:
## ⚡Flujo de Trabajo
### 🛠️ 1. Clonar y preparar el entorno:
Abre tu terminal y ejecuta:
```bash
  git clone <URL_DEL_REPO>``
  cd nombre_repo
```
Esto descarga el proyecto en tu máquina localmente.

### 🔄 2. Actualiza siempre el repo antes de trabajar:
Antes de trabajar tus cambios asegúrate de tener actualizado el repo:
```bash
  git checkout main
  git pull origin main
```
🔄Con esto te aseguras de trabajar con la versión más reciente.

### ⛓️ 3. Crea una nueva rama:
Para evitar conflictos, trabaja en una rama nueva:
```bash
  git checkout -b feature-nombre
```
Ejemplo:
```bash
  git checkout -b feature-agregandoLogin
```
#### 💡Recuerda:
Usa nombres descriptivos en tus ramas.

### 📤 4. Realiza cambios y súbelos:
Cuando hayas terminado de trabajar tus modificaciones:
```bash
  git add .
  git commit -m "Descripción de los cambios"
  git push origin feature-nombre
```
Ejemplo:
```bash
  git commit -m "Añadiendo formulario de inicio de sesión"
  git push origin feature-agregandoLogin
```
🚀Tu código ahora está en GitHub.

### 🔍 5.Espera revisión:
El administrador revisará tu código y hará merge a la rama principal.

## ❗ IMPORTANTE:

🚫 No hagas cambios en la rama main directamente.

🗣️ Comunica al equipo siempre que hagas un cambio.

💬 Si tienes dudas, comunícate con el administrador antes de subir cambios.

🔄 Siempre actualiza tu código antes de empezar una nueva tarea.
