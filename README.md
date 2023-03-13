# Winlocker
A simple Winlocker on C#

## Disclaimer
Данный материал был взят из свободного доступа и предоставлен только в ознакомительных целях! Я не побуждаю повторять все действия и ответственности за это не несу!

### Что из себя представляет данный Winlocker?
По факту, мой Winlocker - это самое обычное окно с виртуальной клавиатурой, у которого установлен стиль отображения "поверх всех" и который осуществляет следующие задачи:
1. Блокировку событий ввода с клавиатуры.
2. Блокировку запуска диспетчера задач.
3. Блокировку запуска редактора реестра.
4. Блокировку правой кнопки мыши.
5. Блокировку кнопки перезагрузки в пространстве ctrl + alt + delete.
6. Добавление в автозагрузку.
7. Проверку на наличие прав локального администратора.
8. Скрытие панели задач.

### Разблокировка
Разблокировка осуществляется вводом правильного пароля, при нажатии на виртуальные клавиши. После ввода правильного пароля происходит откат изменений в реестре Windows и закрытие формы.
В файле Form1.cs и в методе button_unlock_Click установите свой пароль:
```
private void button_unlock_Click(object sender, EventArgs e)
{
    String password = textBox.Text;

    if (password == "tima") // Пароль для разблокировки
    {
        try
        {
            TaskManager.Unlock();
            Registry_editor.Unlock();
            Right_click.Unlock();
            Rebooting.Unlock();
            Autorun.Unset();
            TaskBar.Unlock();
            this.Close();
        }
        catch
        {
            MessageBox.Show(
            "Разблокировка не удалась",
            "Unlocking",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button1,
            MessageBoxOptions.DefaultDesktopOnly);
        }
    }
    else
    {
        textBox.Clear();
    }
}
```
