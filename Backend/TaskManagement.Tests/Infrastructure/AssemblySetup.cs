// Test sınıfları varsayılan olarak paralel çalışır.
// İki WebApplicationFactory aynı anda Serilog log dosyasını kilitleyince crash olur.
// DisableTestParallelization ile sıralı çalışma garanti edilir.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
