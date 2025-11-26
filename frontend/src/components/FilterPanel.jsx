function FilterPanel({ exercises, onFilter, showPRsOnly, setShowPRsOnly }) {
  return (
    <div className="filter-panel">
      <div className="filter-group">
        <label>Exercise:</label>
        <select onChange={(e) => onFilter('exercise', e.target.value)}>
          <option value="">Alle</option>
          {exercises.map(ex => (
            <option key={ex} value={ex}>{ex}</option>
          ))}
        </select>
      </div>

      <div className="filter-group">
        <label>Periode:</label>
        <select onChange={(e) => onFilter('period', e.target.value)}>
          <option value="all">Alles</option>
          <option value="week">Laatste week</option>
          <option value="month">Laatste maand</option>
          <option value="year">Dit jaar</option>
        </select>
      </div>

      <div className="filter-group">
        <label>
          <input
            type="checkbox"
            checked={showPRsOnly}
            onChange={(e) => setShowPRsOnly(e.target.checked)}
          />
          Alleen PRs
        </label>
      </div>
    </div>
  );
}

export default FilterPanel;