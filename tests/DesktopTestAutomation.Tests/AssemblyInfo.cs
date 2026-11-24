// Masaüstü UI testleri tek bir masaüstünü (fare, klavye, odak) paylaştığı için
// aynı makinede paralel çalıştırılamaz.
[assembly: Parallelizable(ParallelScope.None)]
