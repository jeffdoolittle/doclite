doclite
=======

a simple esent backed document store for .net
---------------------------------------------

Example:

```
	var store = new SessionFactory();
	store.StoreAt(Location);
	store.Compress();
	store.EncryptWithKey("ABCDEFGHIJKLMNOP");
	store.Initialize();

	using (var session = store.OpenSession())
	{
		session.Add(new MyDocument
			{ 
              Id = 4,
    			Name = "some value",
				Number = 2
			});	
	}

	store.Dispose();


```