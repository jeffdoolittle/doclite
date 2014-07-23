doclite
=======

a simple esent backed document store for .net
---------------------------------------------

Example:

```
var store = new SessionFactory(
        cfg => cfg
            .StoreAt("/path")
            .Compress()
            .EncryptWithKey("ABCDEFGHIJKLMNOP"));

	using (var session = store.OpenSession())
	{
		session.Add(new MyDocument
			{ 
              Id = 4,
    			Name = "some value",
				Number = 2
			});	
	}

	using (var session = store.OpenSession())
	{
        var document = session.Get<MyDocument>(4);
    }

	store.Dispose();
```