# Документация к проекту OADataService

Проект создает сервис данных, удобный для построения информационных систем разного назначения. В наибольшей степени проект ориентирован на фактографический подход, когда фиксируются только факты, но не интерпретации фактов. Сервис данных предоставляет возможности сохранения документов в виде файлов достаточно произвольного формата, напр. фото-видео-аудио-DOC-PDF и др. Кроме того, можно сохранять и модифицировать базу данных формата RDF. Носитилем базы данных являются так называемые фог-файлы - файлы с информацией об RDF-графе, см. дальше. В совокупности, сервис является базой данных и документов. Функциональность сервиса заключается в оперативном доступе к данным и документам, возможности добавлять, уничтожать и изменять данных, возможности добавлять и уничтожать документы. Сервис реализован средствами HTTP, предполагает удаленный доступ клиентов, ограничивает клиентов в доступе по записи.

### Конструкция кассеты
И данные и документы (файлы), сохраняемые в системе, группируются в так называемые кассеты. Пока поддерживается единственный формат кассеты. Кассета представляет собой директорию файловой системы, имеющую имя и некоторую конструкцию. Имя директории совпадает с локальным именем кассеты (в примерах будем использовать cassname). Полное имя кассеты будет показано дальше. Кассета состоит из файла cassette.finfo и трех директорий: originals, meta, documents. В директории meta распологается фог-файл с именем cassname_current.fog, содержащий базу данных данной кассеты. 

В директории originals хранятся оригиналы хранимых документов. Для этого, в директории заводятся поддиректории 0001, 0002, ... (сколько потребуется), а уже в них, помещаются оригиналы хранимых документов под искусственными именами: 0001.ext, 0002.ext, ... 9999.ext, где номер - номер хранимого документа в данной директории, ext - расширение файла, взятое из оригинала. Реальные имена и метаинформация о файлах, хранятся в базе данных, см. далее. 

В директории documents хранятся некоторые преобразованные в другие типоразмеры документы. Там есть папки с названиями small, medium, normal, устроенными также как и originals. В них, если требуется, под теми же номерами подпапок и файлов хранятся другие варианты того же документа. Например, документу cassname/originals/0002/0033.tiff будут соответствовать документы cassname/documents/small/0002/0033.jpg, cassname/documents/medium/0002/0033.jpg, cassname/documents/normal/0002/0033.jpg, являющиеся уменьшиными преобразованиями, более удобными для визуализации в Интернете. В нынешней практике используются типоразмеры 120, 640, 1200.     

Среди хранимых в кассете документов могут быть фог-документы. Фог-документы, с одной стороны являются хранимыми документами наряду с другими, с другой стороны, они являются носителями (распределенной) базы данных. База данных содержит информацию и о внешних сущностях (персоны, организационные системы, геосистемы) и о хранимых документах, а также связывает их между собой через различные отношения.

### Сервис данных
Сервис данных OADataService, опираясь на заданный набор кассет, формирует некоторое поле данных и некоторую функциональность. Формируемое информационное поле состоит из документов и данных. Данные - это некоторые формализованные записи о сущностях и связях между сущностями. Документы - это, с одной стороны данные, с другой стороны это контент хранимых документов, т.е. набор (хранимых) байтов, составляющих файл документа. Единицей данных является запись, единицей контента документа является файл. Пока реализована схема один документ - один файл. В дальнейшем, предполагается расширение подхода на многодокументные сборки типа CD и архивов и, возможно, многофайловые документы.

У единиц хранения адресом является идентификатор. Теоретически, идентификатор должен быть уникальным в глобальном контексте, практические вопросы формирования уникальных идентификаторов обсуждаются далее. У контента (файлов докоментов) идентификация ведется через так называемый uri. Также предполагается глобальный контекст уникальности таких идентификаторов. 

Базовая функциональность сервиса заключается с том, чтобы по идентификатору получать запись из базы данных и по uri получать контент документа. Также все основные классы сущностей (не отношения) снабжаются полем name, содерждащим имя объекта. Дополнительной функциональностью является получение всех записей с похожим на образец значением поля name. 

### Документы
Документы являются двойственными по своей сути. С одной стороны, документ - это его контент, который можно использовать в разных целях, напр. для просмотра. С другой строны, документ, как и любой другой объект зафиксированый в хранилище, это запись (можно сказать - карточка) метаинформации об этом объекте. К метаинформации относятся имя документа, дата создания документа и другие поля, которые будут детальнее описаны в разделе "Онтология". Особую роль играют для записи (карточки) документа такие поля как uri и docmetainfo. Первое поле связывает карточку с хранимым контентом, второе - группирует специальную метаинформацию, существенную для файла контента, такую как размеры, contenttype, разрешающие способности и т.д. Используя идентификатор запись (карточку), в базе данных устанавливаются также отношения между  документом и другими сущностями. 

Разные виды документов зачастую требуют разного доступа к ним. Это связано с использованием документов для решения разных задач. Базовым доступом к контенту документа является получение копии файла этого документа (параметр u - uri): 
```
    [HttpGet("docs/GetDoc")]
    public IActionResult GetDoc(string u); // content-type: application/octet-stream
```
Это требуется для фактически ручных последующих операций. Но документ, особенно мультимедиа, часто интересен тем, что его можно посмотреть через базовые средства просмотра в браузере или работы с сервисом, поэтому используется вариант запроса, порождающих поток вывода определенного типа контента:
```
    [HttpGet("docs/GetPhoto")]
    public IActionResult GetPhoto(string u, string s); // content-type: image/jpg
    [HttpGet("docs/GetVideo")]
    public IActionResult GetVideo(string u); // content-type: video/mp4
    [HttpGet("[controller]/GetPdf")]
    public IActionResult GetPdf(string u); // content-type: application/pdf
```

В общем случае, хранимые документы являются изменяемыми. Это в большей части касается фог-документов (см. далее) и в меньшей документов других видов. В любом случае, процесс изменения документа является регламентированным, с попыткой максимально сохранять свойство фактографических систем "что написано пером, не вырубишь и топором". 

### Фог-документы
Фог-документы или Fog-документы (от слова Factograph) - носители базы данных. Это построение почти полностью соответствует концепциям RDF, хотя есть и различия (см. далее). Фог-документы являются наборами записей. Запись - структурированная совокупность триплетов RDF, объединенная по субъекту. Идентификатор субъекта будем называть идентификатором записи. Пример записи в формате N3:
```
<p123456789> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://fogid.net/o/person> ;
    <http://fogid.net/o/name> "Пупкин Василий Васильевич" ;
    <http://fogid.net/o/from-date> "1980-04-01 .
```
Запись определяет запись о сущности с идентификатором p123456789, имеющую тип персоны и имеющую два поля - имя и дату рождения. 

Возможно много форм текстовой сериализации RDF, по историческим причинам мы пользуемся XML-представлением, напр. предыдущая запись будет выглядеть:
```
<person rdf:about="p123456789" xmlns="http://fogid.net/o/" xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#" >
    <name>Пупкин Василий Васильевич</name>
    <from-date>1980-04-01</from-date>
</person>
```

К сожалению, XML-сериализация имеет существенные недостатки, связанные с оформлением использования пространств имен (namespaces). В дальнейшем предполагается использование более эффективных форматов, но пока все сделано в XML. Соответственно, сам fog-документ выглядит как элемент с именем rdf:RDF, группирующий отдельные записи. 
```
<rdf:RDF xmlns="http://fogid.net/o/" xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#" >
    <person rdf:about="p123456789">
        <name>Пупкин Василий Васильевич</name>
        <from-date>1980-04-01</from-date>
        <father rdf:resource="p1111111" />
    </person>
    <person rdf:about="p1111111">
        <name>Пупкин Василий Иванович</name>
        <from-date>1950</from-date>
    </person>
    ...
</rdf:RDF>
```

Обратим внимание на то, что в кассете может быть произвольное количество фог-документов (фогов), а сервис может объединять произвольное количество кассет. Причем, в общем случае, кассет размещенных в разных частях Интернета. Конкретный сервис может объединять не все кассеты, а лишь некоторое подмножество, это считается корректным несмотря на то, что часть информации будет "утеряна" и часть ссылок станет "подвисшими". Логика получающегося построения сведена к логике распределенного key-value хранилища. Применительно к фог-документам, указанная логика может быть сформуирована в следующих тезисах:  

База данных представляет собой множество записей, содержащихся в фогах, вовлеченных в построение базы данных. Записи, с одинаковыми идентефикаторами, соответствуют одной и той же сущности, среди этих записей в базе данных используется лишь одна. Для определения текущего оригинала, используется атрибут записи http://fogid.net/o/mT - отметка времени. "Побеждает" более поздняя запись. Этот механизм позволяет решать важную проблему: изменение записи созданием записи в другом документе. Кроме того, возможно сохранение старого значения для потенциального использования. Например, в одной конфигурации сервиса данных будет "работать" новое определение, если фог-документ с новым определением загружено и будет использоваться старое, если новое определение не загружено. Как и в других темпоральных базах данных, простановка отметки времени осуществляется автоматически.  

Изменения в базе данных заключаются в добавлении в какой-то фог-документ новой записи, т.е. записи с новым, не встречавшимся идентификатором, в изменении имеющихся записей, в уничтожении записей. Добавление выполняется простым добавлением новой записи в активный набор, изменение осуществляется так же, только добавляется запись под существующим идентификатором, но с более поздней временной отметкой. Уничтожение выполняется аналогично изменению, но добавляемая запись должна бять специального типа http://fogid.net/o/delete. 

### Запросы и ответы к базе данных

Основные запросы к базе данных:
```
    [HttpGet]
    public ContentResult GetItemByIdBasic(string id, string addinverse);
    [HttpGet]
    public ContentResult SearchByName(string ss, string tt);
    [HttpPost]
    public ContentResult GetItemById(string id, string format);
    [HttpPost]
    public ContentResult PutItem(string item);
    [HttpPost]
    public ContentResult UpdateItem(string item);
```
Причем результатом запроса являются одиночные XML-элементы, во втором случае это элемент <results>...</results>, группирующий элементы - результаты поиска. В последних двух Post-запросах аргументом подается внешне сформированная запись. Новая запись добавляется как новая или замещает существующее значение с заданным идентификатором или изменяет существующее значение. Изменение значения заключается в том, что кроме новых триплетов, в выходной результат попадают те старые, которые имеют незадействованные в новых предикаты. 

Формат результатов первых трех запросов не совпадает с ранее описанными форматами. 

### Общая архитектура сервиса

Конфигурация сервиса задается файлом config.xml, в котором указываются кассеты, предназначенные для формирования объединенного информационного поля. Кассеты, как уже формулировалось, содержат фог-документы, которые загружаются в сервис и там формируется текущая база данных. Текущая база данных является информационным образованием, оптимизированным на эффективное выполнение основных операций доступа к данным. С точки зрения сохранения и обработки, текущая база данных является вторичной, а первичной является набор фог-документов. В дальнейшем, изменения в данных выполняются параллельно - в фог-документах и в текущей базе данных. Формат конфигуратора достаточно прост:
```
<config>
    <database connectionstring="протокол:строка подключения" />

    <LoadCassette>Путь к директории кассеты 1</LoadCassette>
    <LoadCassette>Путь к директории кассеты 2</LoadCassette>
    ...
</config>
```
Собственно конфигуратор определяет именно эти два элемента - базу данных через строку подключения и набор кассет. Строка подключения (connection string) задает адаптер базы данных (протокол) и параметры к адаптеру.