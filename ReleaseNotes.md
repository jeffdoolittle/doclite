### New in 0.0.2 (Release 2014/07/23)
* Updated README to reflect actual SessionFactory implementation
* Improved Key generation to ensure proper sorting when using long, int, short or byte ids
* Added First<T>() to ISession to fetch the first document of a type
* Added Last<T>() to ISession to fetch the last document of a type
* Added Get<T>(object[] id) to ISession for retrieving documents for multiple keys
* Added Get<T>(int skip, int take) to ISession for document paging
* Added performance and unit test coverage

### New in 0.0.1.1 (Release 2014/07/22)
* Fixed nuget package (Added assembly to 'lib' folder)

### New in 0.0.1 (Released 2014/07/22)
* First release of DocLite.