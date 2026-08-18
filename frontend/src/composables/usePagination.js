import { ref, computed } from 'vue'

export function usePagination(allRows, defaultSize = 10) {
  const currentPage = ref(1)
  const pageSize = ref(defaultSize)
  const pageSizes = [10, 20, 50, 100]

  const total = computed(() => allRows.value.length)

  const displayRows = computed(() => {
    const start = (currentPage.value - 1) * pageSize.value
    return allRows.value.slice(start, start + pageSize.value)
  })

  function resetPage() {
    currentPage.value = 1
  }

  function handleSizeChange() {
    currentPage.value = 1
  }

  return { currentPage, pageSize, pageSizes, total, displayRows, resetPage, handleSizeChange }
}
