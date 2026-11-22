using System;
using System.Collections.Generic;
using System.Globalization;

namespace DeskBloom {
  internal static class L {
    internal static string Language=SystemLanguage;
    internal static string SystemLanguage {get{return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName=="ru"?"ru":"en";}}
    internal static readonly Dictionary<string,string> English=new Dictionary<string,string> {
      {"Оформление группы…","Group appearance…"}, {"Оформление группы","Group appearance"},
      {"Цвет","Color"}, {"Цвет значка и кнопки · 30 цветов","Icon and button · 30 colors"},
      {"Фон группы · 30 цветов","Group background · 30 colors"}, {"Значок группы · 15 значков","Group icon · 15 icons"},
      {"Значок группы","Group icon"}, {"Предпросмотр группы","Group preview"},
      {"Загрузить изображение…","Upload image…"}, {"Изображения","Images"},
      {"Изображение масштабируется автоматически. Исходный файл сохраняется.","Images are scaled automatically. The original file is preserved."},
      {"Папка служебных данных занята.","The metadata folder is already occupied."},
      {"Жирный","Bold"}, {"Курсив","Italic"}, {"Подчёркнутый","Underline"},
      {"Сбросить форматирование","Reset formatting"},
      {"Выделите текст, затем выберите цвет или стиль.","Select text, then choose a color or style."},
      {"Введите допустимое имя папки: от 1 до 100 символов, без / \\ : * ? \" < > |.","Use a valid folder name: 1–100 characters, without / \\ : * ? \" < > |."},
      {"Прокрутить файлы","Scroll files"},
      {"Загрузка…","Loading…"},
      {" · файлов и папок"," · files and folders"},
      {" и "," and "},
      {"\nДвойной клик — открыть. Перетаскивание — передать файл.","\nDouble-click to open. Drag to share the file."},
      {"+ Папка","+ Folder"}, {"+ Файлы","+ Files"},
      {"Desk Bloom — группы","Desk Bloom — groups"}, {"Desk Bloom · группы","Desk Bloom · groups"},
      {"Вернуть на рабочий стол","Return to desktop"},
      {"Всё нужное — в одной группе.\nДобавьте файлы или целую папку.","Everything you need, in one group.\nAdd files or an entire folder."},
      {"Выбери группу для переноса файлов","Choose a group to move the files into"},
      {"Выберите папку на том же диске для переноса группы.","Choose a folder on the same drive to move this group."},
      {"Выход","Exit"}, {"Где хранить обычную папку группы?","Where should this group's folder be stored?"},
      {"Группа ","Group "}, {"Группа Desk Bloom","Desk Bloom group"}, {"Группа","Group"},
      {"Добавить в группу Desk Bloom…","Add to a Desk Bloom group…"},
      {"Добавить в группу Desk Bloom","Add to a Desk Bloom group"},
      {"Добавить в другую группу…","Move to another group…"},
      {"Добавить файлы (перенести)","Add files (move)"},
      {"Добавить целую папку (перенести)","Add an entire folder (move)"},
      {"Добавьте существующий ярлык установленной программы; её системные файлы должны оставаться на месте.","Add an existing shortcut to the installed app; its system files must stay in place."},
      {"Запускать вместе с Windows","Start with Windows"}, {"Мои группы","My groups"},
      {"Нажатие — открыть / закрыть. Перетаскивание — переместить. ПКМ — меню.","Click to expand or collapse. Drag to move. Right-click for options."},
      {"Название группы","Group name"}, {"Не все файлы удалось перенести","Some files could not be moved"},
      {"Не найдено: ","Not found: "},
      {"Не удалось однозначно восстановить старый перенос.","The previous move could not be safely recovered."},
      {"Не удалось прочитать настройки. Существующие папки и файлы сохранены.","Settings could not be read. Your existing folders and files are preserved."},
      {"Некорректный идентификатор группы.","Invalid group identifier."},
      {"Некорректный путь в старом журнале переноса.","Invalid path in a previous move journal."},
      {"Нельзя переместить группу внутрь самой себя.","A group cannot be moved inside itself."},
      {"Нельзя перенести эту папку или сам органайзер в группу.","This folder or the organizer itself cannot be moved into a group."},
      {"Неоднозначный перенос. Файлы сохранены: ","Move recovery is ambiguous. Files are preserved at: "},
      {"Новая группа","New group"},
      {"Откройте файл .groups двойным щелчком, чтобы добавить саму группу.","Double-click the .groups file to add the group itself."},
      {"Открыть папки групп","Open group folders"}, {"Открыть папку группы","Open group folder"},
      {"Открыть","Open"}, {"Отмена","Cancel"},
      {"Папка группы не найдена: ","Group folder not found: "}, {"Переименовать","Rename"},
      {"Переместить папку группы…","Move group folder…"},
      {"Перенести файлы в группу","Move files into this group"},
      {"Перенести целую папку в группу","Move an entire folder into this group"}, {"Перенести","Move"},
      {"Перетащите сюда файлы и папки","Drop files and folders here"},
      {"Показать в Проводнике","Show in File Explorer"},
      {"Показать группы на рабочем столе","Show groups on desktop"},
      {"Раскрыть группу","Expand group"}, {"Свернуть группу","Collapse group"}, {"Сменить цвет","Change color"},
      {"Сначала необходимо восстановить старое переключение страниц.","A previous desktop switch must be recovered first."},
      {"Создать группу Desk Bloom в этой папке","Create a Desk Bloom group in this folder"},
      {"Создать группу Desk Bloom","Create a Desk Bloom group"}, {"Создать группу","Create group"},
      {"Сохранить","Save"}, {"Убрать группу, сохранив файлы","Remove group, keep files"},
      {"Убрать панель группы? Файлы останутся в папке:\n","Remove this group panel? Your files will stay in:\n"},
      {"Это не файл группы Desk Bloom.","This is not a Desk Bloom group file."}
    };
    internal static string T(string russian){string translated;return Language=="en"&&English.TryGetValue(russian,out translated)?translated:russian;}
    internal static string Items(int count){if(count==0)return T("Перетащите сюда файлы и папки");return Language=="en"?count+(count==1?" item":" items"):count+" · файлов и папок";}
  }
}
