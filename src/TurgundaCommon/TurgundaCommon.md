# Заметки по проекту TurgundaCommon

В принципе, эта библиотека предоставляет общие средства для работы с объектами Тургунды. Можно сказать, что это  интерфейс хранилища данных data storage. Что в интерфейсе есть? 

ONames.cs: онтологические имена. 
ModelCommon.cs: Онтологические имена и прилегающая информация. Можно сказать, онтологическая "кухня" - словари, переводы слов, загрузка онтологий  

SObjects.cs: Статические объекты хранилища. Работа с accounts, вспомогательные переменные и настройки. Главное - один движок engine класса CassetteIntegration.  В нем - "самый главный" статический метод редактирующей работы с хранилищем 
```
    public static XElement PutItemToDb(XElement item, bool tocreateid, string username);
```
База данных содержит множество идентифицированных айтемов. 
Метод предполагает, что в хранилище помещаются айтемы. Айтем идентифицируется атрибутом rdf:about, если он есть. Это два варианта: есть: а) и нет: б). 
Вариант б) предполагает, что генерируется новый идентификатор, добавляется к указанному айтему, айтем помещается в базу данных, а процедура возвращает сгенерированный идентификатор. Вариант а) предполагает добавление айтема к базе данных, если айтем с этим идентификтором уже существовал, то предполагается вариант редактирования, когда новый айтем или "убивает" старое значение, либо "маскирует" его. Если имя айтема "{http://fogid.net/o/}delete", то айтем с указанным идентификатором уничтожается или маскируется. Это не должно препятствовать дальнейшему появляению айтема через следующие операторы. 

На самом деле, есть еще имя пользователя username. Что оно означает и как оно используется? Оно означает текущую идентификацию пользователя, посылающего оператор редактирования. Все множество айтемов распадается на подмножества (fog-докумненты). Причем каждое подмножество имеет единое имя владельца (пользователя) и единый режим редактирующего доступа владельца к fog-документу. Редактирующее изменение, запрошенное оператором PutItemToDB произойдет, если  существует хотя бы один fog-документ, в котором владелец тот же, что и username и полномочия на изменение документа имеются. В этом случае, изменнение будет внесено в соотвествующий fog-документ в виде нового айтема или редактирования имеющегося айтема с данным идентификатором. Для того, чтобы в дальнейшем выявлять более поздний айтем, каждый айтем снабжается временной отметкой. Уничтожение айтема всегда (!) должно происходить как редактирование с сохранением последней временной отметки. 

Есть еще механизм замен. Может появиться оператор substitute с атрибутами old-id и new-id, "сливающий" два множества айтемов с разными идентификаторами в одно с назначением в качестве общего идентификатора new-id. В этом массовом действии есть скрытые семантические проблемы. Например, если есть объединительная запись, но потом она не попала в базу данных, то айтемы могут снова "распасться" и уже уничтоженные айтемы могут снова появиться. Правда это возможно и для отстутствия оператора substitute. Есть и другая проблема оператора. Она заключается в том, что идентификаторы, напр. old-id накапливаются не только в базе данных, но и во "внешнем мире" и там их трудно переделать. Это ограничивает возможности прямого переименования old-id -> new-id. 

Технически, подстановка может осуществляться таблицей id -> id, указывающей подмену. При наличии большого количества идентификаторов, эта таблица может оказаться довольно громозкой. Однако, если исходить из предположения, что таких подстановок малое, относительно общего, число, такую таблицу можно построить только для переименованных идентификаторов. Все остальные построения и действия могут быть сохранены как для предыдущего случая.  Можно также не допускать динамического переименования (подстановки), тогда таблица будет формироваться только при загрузке базы данных. Или ее дозагрузки. 

Еще существенным моментом является синхронизация. Вполне возможным является использование критического интервала через одну синхронизационную переменную. Причем и на редактирование и на неизменяющее чтение. 

### 20181113 11:31
Я вчера не смог придумать схему более экономную в синхронизации, чем общий критический интервал. Все кажется, что при доминировании доступа по чтению, можно было бы использовать этот факт. Пусть есть булевcкая переменная типа read_access_awailable. Если эта переменная истина, то можно без ограничений выполнять операции доступа, имея ввиду, что запись или модификация данных не ведется. Если требуется начать запись/модификацию, то можно эту булевскую переменную установить в ложь. И тогда можно было бы начать синхронизацию операций записи/модификации, так же как и чтения/неизменяющего доступа, через критический интервал. Проблема заключается в том, что у нас нет механизма выяснения того, когда предыдущий поток операций неизменяющего доступа закончится, чтобы уже начать синхронизацию. Возможно будет полезно, в этой задаче использовать семафоры. Обнуление семафора и будет таким сигналом о переходе в режим записи. 

Вернусь к модели слияния (факторизации?) имен. Сначала попробую разобраться с ситуацией без фаторизации. Есть айтемы, у которых есть идентификаторы и есть временные отметки. Возьмем "классическую" последовательность айтемов. Построим простейший индекс и будем выбирать по ключу все айтемы, а потом находить с максимальной временной отметкой. Будет такая конструкция работать? Будет! Более того, если разбить множества айтемов на подмножества, то также будет! Только появится еще один поиск максимальной временной отметки теперь уже по подмножествам. 

Теретически, временную отметку можно было бы заменить на порядок в последовательности, поскольку ее рост выполняется в "хвост". Однако, это ограничит использование подмножеств. В принципе, подключение кассет может быть динамическим в том смысле, что в кассете может быть своя развернутая база данных. По крайней мере, прямая задача key-value так решается. Но есть и обратная задача: по заданному идентификатору, найте все записи, в которых он присутствует в позиции ссылки. Эта задача также распадается на подзадачи для подмножеств айтемов и общее решение - объединение частных решений. Однако, как же технически решать ее для случая наличия временных меток. Можно никак не решать. Можно через векторый индекс. Можно через идентификаторы айтемов и их объединение. Так пожалуй - идейнее...

### 20181115 11:42
Попробую оценить насколько вопрос слияния (факторизации) имен актуальный в контексте Открытого архива. Для этого, надо промерить загрузку всех данных в OA на сервере.

Посмотрел на реализацию ri_table, ответственную за слияние. Выглядит тяжеловесно... Возможно, было бы правильнее использовать хеш-функцию и битовую шкалу. Например, проверяем какой-то идентификатор, получаем целое через хеш-функцию, отмечаем позицию в битовой шкале. Если позиция "занята" предыдущей отметкой, то выделяем это значение список. При повторном сканировании для всех идентификаторов, попавших в список, строим какое-то интересное построение. 

Проверил. Действительно, в базе данных открытого архива есть более 14 тыс. уничтожений, но нет подстановок. Поскольку пользователи не просят, видимо и нет актуальности этого механизма. Операторы delete можно заменить на что-нибудь более естественное. Например, на полностью пустую запись, т.е. есть только rdf:about и отметка времени. 

Одна из мыслей, которая меня беспокоит, это куда девать информацию об создателе записи owner. Можно заграть эту информацию в поле записи. Можно записи разбить по создателям. Технически, на кассетном уровне, делается именно это. Еще одна мысль - как бы уменьшить необходимость делать запрос ко всем fog-документам? Предпосылка такой возможности в том, что основа локальной базы данных идентифицируется "своими" идентификаторами. Но есть и "чужие". Можно было бы выделить все эти "чужие" идентификаторы в единую часть, например, одну на кассету и набор тестируемых документов бы резко сократился. 

Попробую сделать некоторое "классическое" решение несмотря на возможные неэффективности. Итак,пусть база данных будет содержать классические "тройки"...

Еще раз подумал и решил, что пока не дозрел до такого решения. Попробую модифицировать решение, основанное на XML-элементах. 

Начал работать. Выявил, что в базе данных Открытого архива сейчас около 407 тыс. элементов. База данных просто грузится по XElement.Load() порядка 3 сек.

Теперь надо формировать базу данных. Причем попробую это сделать не в оперативной памяти. Примитивное решение - просто XML-элемент в текстовом виде. Можно попробовать. 

Попробовал. Запись последовательности всех 407 тыс. Элементов выполняется за приблизительно 8 сек. Это быстро. Теперь надо сделать индексное построение на ключ элемента. 

### 20181116 11:18
Что меня беспокоит? Надежная работа RDF-движка! А еще хочется, чтобы он работал быстро и чтобы холодный старт осуществлялся также быстро. Наверное, проще всего снова сделать движок на базе XML. Только надо бы сделать по-другому уничтожение и замены. Насчет уничтожения - буду двигаться в сторону "стандартного" решения, когда запись заменяется "нулевой". Насчет замен - в Открытом архиве их нет, пока может не заморачиваться. Насчет динамики - можно все сделать на fog-документах. Добавление нового или редактирование старого будет формированием этого элемента, поиском фог-базы для помещения, добавление или замена имеющегося, сохранение в файле. При этом, должны выполняться коррекции таблиц. 

20181120 11:51
Кажется, самым протым движком может быть следующий: 
1) В оперативной памяти разворачивается весь набор fog-документов. 
2) Создается специальная индексная структура в виде словаря, входом которого является идентификатор сущности, значением некоторая запись, включающая в себя fog-запись оригинала и список обратных ссылок. Обратной ссылкой у нас будет элемент с атрибутом rdf:resource, "ведущий" к айтему с текущим идентификатором сущности. 
3) При редактировании айтема, появляется его другое значение. Также оно снабжено отметкой времени и владельцем. По имени владельца, находим текущий активный пользовательский документ. В этом документе находим запись с заданным идентификатором. Если запись есть, то заменяем (!) ее, если нет, то добавляем элемент. "Пустой" элемент изображается элементом с локальным именем delete. 





