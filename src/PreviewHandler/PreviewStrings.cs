using System.Globalization;

namespace JPDFs.PreviewHandler;

/// <summary>
/// The pane's few words in the app's languages, picked from the Windows display language
/// because the preview runs inside the shell rather than the app.
/// </summary>
internal static class PreviewStrings
{
    internal enum Key { FitWidth, FitPage, ZoomOut, ZoomIn, Open, Protected, Failed, Incomplete, Retry }

    private static readonly Dictionary<string, string[]> Tables = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en-US"] = ["Fit width", "Fit page", "Zoom out", "Zoom in", "Open in J-PDF's",
            "This PDF is password protected. Open it in J-PDF's to enter the password.", "This PDF could not be previewed."],
        ["bn"] = ["প্রস্থে মানান", "পৃষ্ঠায় মানান", "ছোট করুন", "বড় করুন", "J-PDF's-এ খুলুন",
            "এই PDF পাসওয়ার্ড দিয়ে সুরক্ষিত। পাসওয়ার্ড দিতে এটি J-PDF's-এ খুলুন।", "এই PDF-এর প্রিভিউ দেখানো যায়নি।"],
        ["cs-CZ"] = ["Přizpůsobit šířce", "Přizpůsobit stránce", "Oddálit", "Přiblížit", "Otevřít v J-PDF's",
            "Toto PDF je chráněno heslem. Otevřete ho v J-PDF's a zadejte heslo.", "Náhled tohoto PDF nelze zobrazit."],
        ["de-DE"] = ["Breite anpassen", "Seite anpassen", "Verkleinern", "Vergrößern", "In J-PDF's öffnen",
            "Diese PDF ist kennwortgeschützt. Öffnen Sie sie in J-PDF's, um das Kennwort einzugeben.",
            "Für diese PDF kann keine Vorschau angezeigt werden."],
        ["es"] = ["Ajustar al ancho", "Ajustar a la página", "Alejar", "Acercar", "Abrir en J-PDF's",
            "Este PDF está protegido con contraseña. Ábralo en J-PDF's para escribir la contraseña.",
            "No se puede mostrar la vista previa de este PDF."],
        ["fr-FR"] = ["Ajuster à la largeur", "Ajuster à la page", "Zoom arrière", "Zoom avant", "Ouvrir dans J-PDF's",
            "Ce PDF est protégé par un mot de passe. Ouvrez-le dans J-PDF's pour saisir le mot de passe.",
            "Impossible d'afficher l'aperçu de ce PDF."],
        ["hu-HU"] = ["Szélességhez igazítás", "Oldalhoz igazítás", "Kicsinyítés", "Nagyítás", "Megnyitás a J-PDF's-ben",
            "Ez a PDF jelszóval védett. Nyissa meg a J-PDF's-ben a jelszó megadásához.",
            "Ennek a PDF-nek nem jeleníthető meg az előnézete."],
        ["it-IT"] = ["Adatta alla larghezza", "Adatta alla pagina", "Riduci", "Ingrandisci", "Apri in J-PDF's",
            "Questo PDF è protetto da password. Aprilo in J-PDF's per inserire la password.",
            "Impossibile visualizzare l'anteprima di questo PDF."],
        ["ja-JP"] = ["幅に合わせる", "ページに合わせる", "縮小", "拡大", "J-PDF's で開く",
            "この PDF はパスワードで保護されています。J-PDF's で開いてパスワードを入力してください。",
            "この PDF をプレビューできませんでした。"],
        ["kk-KZ"] = ["Ені бойынша", "Бет бойынша", "Кішірейту", "Үлкейту", "J-PDF's-те ашу",
            "Бұл PDF құпиясөзбен қорғалған. Құпиясөзді енгізу үшін оны J-PDF's-те ашыңыз.",
            "Бұл PDF-ті алдын ала қарау мүмкін болмады."],
        ["nb-NO"] = ["Tilpass bredde", "Tilpass side", "Zoom ut", "Zoom inn", "Åpne i J-PDF's",
            "Denne PDF-filen er passordbeskyttet. Åpne den i J-PDF's for å skrive inn passordet.",
            "Kan ikke forhåndsvise denne PDF-filen."],
        ["pl-PL"] = ["Dopasuj do szerokości", "Dopasuj do strony", "Pomniejsz", "Powiększ", "Otwórz w J-PDF's",
            "Ten plik PDF jest chroniony hasłem. Otwórz go w J-PDF's, aby wpisać hasło.",
            "Nie można wyświetlić podglądu tego pliku PDF."],
        ["pt-BR"] = ["Ajustar à largura", "Ajustar à página", "Reduzir", "Ampliar", "Abrir no J-PDF's",
            "Este PDF é protegido por senha. Abra-o no J-PDF's para digitar a senha.",
            "Não foi possível visualizar este PDF."],
        ["ru-RU"] = ["По ширине", "По странице", "Уменьшить", "Увеличить", "Открыть в J-PDF's",
            "Этот PDF защищен паролем. Откройте его в J-PDF's, чтобы ввести пароль.",
            "Не удалось показать предварительный просмотр этого PDF."],
        ["sl-SI"] = ["Prilagodi širini", "Prilagodi strani", "Pomanjšaj", "Povečaj", "Odpri v J-PDF's",
            "Ta PDF je zaščiten z geslom. Odprite ga v J-PDF's, da vnesete geslo.", "Predogleda tega PDF-ja ni bilo mogoče prikazati."],
        ["tr-TR"] = ["Genişliğe sığdır", "Sayfaya sığdır", "Uzaklaştır", "Yakınlaştır", "J-PDF's'de aç",
            "Bu PDF parola korumalı. Parolayı girmek için J-PDF's'de açın.", "Bu PDF önizlenemedi."],
        ["uk-UA"] = ["За шириною", "За сторінкою", "Зменшити", "Збільшити", "Відкрити в J-PDF's",
            "Цей PDF захищено паролем. Відкрийте його в J-PDF's, щоб ввести пароль.",
            "Не вдалося показати попередній перегляд цього PDF."],
        ["vi-VN"] = ["Vừa chiều rộng", "Vừa trang", "Thu nhỏ", "Phóng to", "Mở trong J-PDF's",
            "PDF này được bảo vệ bằng mật khẩu. Hãy mở trong J-PDF's để nhập mật khẩu.", "Không thể xem trước PDF này."],
        ["zh-CN"] = ["适合宽度", "适合页面", "缩小", "放大", "在 J-PDF's 中打开",
            "此 PDF 受密码保护。请在 J-PDF's 中打开并输入密码。", "无法预览此 PDF。"],
        ["zh-TW"] = ["符合寬度", "符合頁面", "縮小", "放大", "在 J-PDF's 中開啟",
            "此 PDF 受密碼保護。請在 J-PDF's 中開啟並輸入密碼。", "無法預覽此 PDF。"],
    };

    private static readonly Dictionary<string, string[]> PageProblems = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en-US"] = ["This page could not be displayed completely.", "Retry"],
        ["bn"] = ["এই পৃষ্ঠাটি সম্পূর্ণভাবে দেখানো যায়নি।", "আবার চেষ্টা করুন"],
        ["cs-CZ"] = ["Tuto stránku nelze zobrazit úplně.", "Zkusit znovu"],
        ["de-DE"] = ["Diese Seite konnte nicht vollständig angezeigt werden.", "Erneut versuchen"],
        ["es"] = ["No se pudo mostrar esta página por completo.", "Reintentar"],
        ["fr-FR"] = ["Cette page n’a pas pu être affichée entièrement.", "Réessayer"],
        ["hu-HU"] = ["Ez az oldal nem jeleníthető meg teljes egészében.", "Újra"],
        ["it-IT"] = ["Non è stato possibile visualizzare completamente questa pagina.", "Riprova"],
        ["ja-JP"] = ["このページを完全に表示できませんでした。", "再試行"],
        ["kk-KZ"] = ["Бұл бетті толық көрсету мүмкін болмады.", "Қайталау"],
        ["nb-NO"] = ["Denne siden kunne ikke vises fullstendig.", "Prøv igjen"],
        ["pl-PL"] = ["Nie można wyświetlić tej strony w całości.", "Spróbuj ponownie"],
        ["pt-BR"] = ["Não foi possível exibir esta página por completo.", "Tentar novamente"],
        ["ru-RU"] = ["Не удалось полностью отобразить эту страницу.", "Повторить"],
        ["sl-SI"] = ["Te strani ni bilo mogoče prikazati v celoti.", "Poskusi znova"],
        ["tr-TR"] = ["Bu sayfa tam olarak görüntülenemedi.", "Yeniden dene"],
        ["uk-UA"] = ["Не вдалося повністю відобразити цю сторінку.", "Повторити"],
        ["vi-VN"] = ["Không thể hiển thị đầy đủ trang này.", "Thử lại"],
        ["zh-CN"] = ["无法完整显示此页面。", "重试"],
        ["zh-TW"] = ["無法完整顯示此頁面。", "重試"]
    };

    private static readonly string Language = Resolve(CultureInfo.CurrentUICulture);
    private static readonly string[] Current = Tables[Language];

    internal static string Get(Key key) => key >= Key.Incomplete
        ? PageProblems[Language][(int)key - (int)Key.Incomplete] : Current[(int)key];

    internal static string Resolve(CultureInfo culture)
    {
        string name = culture.Name;
        if (name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
            return name.Contains("Hant", StringComparison.OrdinalIgnoreCase) || name.EndsWith("-TW", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("-HK", StringComparison.OrdinalIgnoreCase) || name.EndsWith("-MO", StringComparison.OrdinalIgnoreCase)
                ? "zh-TW" : "zh-CN";
        if (Tables.ContainsKey(name)) return name;
        string language = culture.TwoLetterISOLanguageName;
        foreach (string candidate in Tables.Keys)
            if (candidate.Equals(language, StringComparison.OrdinalIgnoreCase)
                || candidate.StartsWith(language + "-", StringComparison.OrdinalIgnoreCase))
                return candidate;
        return "en-US";
    }
}
