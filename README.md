# Mensa Quiz

Aplicación de escritorio desarrollada en **WPF (.NET)** para crear y presentar quizzes en clase o cualquier entorno presencial. A diferencia de herramientas como Kahoot, el presentador proyecta las preguntas en pantalla y los participantes responden de forma presencial.

---

## Características

- **Login con dos roles**: Admin y Usuario. Sin registro público, el Admin crea las cuentas.
- **Constructor de quizzes** con 5 tipos de preguntas:
  - Opción múltiple
  - Ordenar elementos
  - Respuesta libre
  - Emparejar pares
  - Pausa / Slide intermedio
- **Modo presentación** — reproduce el quiz pregunta a pregunta, mostrando las respuestas correctas al final de cada una.
- **Historial** de sesiones con estadísticas: participantes, duración, actividad por mes.
- **Panel de administración** — CRUD de usuarios con contraseñas hasheadas con BCrypt.
- **Exportación a PDF** del historial global (solo Admin).
- **Base de datos en VPS** con MariaDB.

---

## Tecnologías

| Capa | Tecnología |
|------|-----------|
| UI | WPF / XAML |
| Patrón | MVVM |
| ORM | Entity Framework Core |
| Base de datos | MariaDB (VPS) |
| Autenticación | BCrypt.Net |
| PDF | QuestPDF |

---

## Requisitos

- .NET 8 o superior
- Visual Studio 2022
- Acceso a una instancia de MariaDB o MySQL

---

## Configuración

1. Clona el repositorio:
```bash
git clone https://github.com/tu_usuario/mensa-quiz.git
```

2. Copia el archivo de configuración de ejemplo:
```bash
cp appsettings.example.json appsettings.json
```

3. Rellena `appsettings.json` con tus datos de conexión:
```json
{
  "ConnectionStrings": {
    "MensaQuiz": "Server=TU_IP;Port=3306;Database=TU_BASE_DE_DATOS;User=TU_USUARIO;Password=TU_PASSWORD;"
  }
}
```

4. Aplica las migraciones:
Update-Database

5. Ejecuta la aplicación. El usuario Admin se crea automáticamente con las credenciales:
Usuario: Admin
Contraseña: Admin

---

## Estructura del proyecto
```
Vazquez_Uribe_Mensa/
├── Documents/          # Generación de PDFs con QuestPDF
├── Models/             # Entidades y DbContext
├── Services/           # Acceso a datos (CRUD)
├── ViewModels/         # Lógica de presentación (MVVM)
│   └── Base/           # BaseViewModel y RelayCommand
├── Views/              # UserControls y MainWindow
├── Images/             # Recursos gráficos
└── appsettings.example.json
```

---

## Autor

**Jordi Vázquez Uribe**  
IES San Vicente — Desarrollo de Aplicaciones Multiplataforma
