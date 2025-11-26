import { useState, useEffect } from 'react';
import axios from 'axios';
import ProgressChart from './components/ProgressChart';
import FilterPanel from './components/FilterPanel';
import './App.css';

const API_URL = 'http://localhost:5274/api';

function App() {
  const [workouts, setWorkouts] = useState([]);
  const [filteredWorkouts, setFilteredWorkouts] = useState([]);
  const [exercises, setExercises] = useState([]);
  const [stats, setStats] = useState(null);
  const [selectedExercise, setSelectedExercise] = useState('');
  const [showPRsOnly, setShowPRsOnly] = useState(false);
  const [filters, setFilters] = useState({ exercise: '', period: 'all' });
  const [form, setForm] = useState({ name: '', reps: '', weight: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    fetchWorkouts();
    fetchExercises();
  }, []);

  useEffect(() => {
    applyFilters();
  }, [workouts, filters, showPRsOnly]);

  const fetchWorkouts = async () => {
    try {
      const res = await axios.get(`${API_URL}/workouts`);
      setWorkouts(res.data);
    } catch (err) {
      setError('Kon workouts niet laden');
    }
  };

  const fetchExercises = async () => {
    try {
      const res = await axios.get(`${API_URL}/exercises`);
      setExercises(res.data);
    } catch (err) {
      console.error(err);
    }
  };

  const fetchStats = async (exerciseName) => {
    try {
      const res = await axios.get(`${API_URL}/stats/${exerciseName}`);
      setStats(res.data);
    } catch (err) {
      console.error(err);
    }
  };

  const applyFilters = () => {
    let filtered = [...workouts];

    if (filters.exercise) {
      filtered = filtered.filter(w => w.name === filters.exercise);
    }

    if (filters.period !== 'all') {
      const now = new Date();
      const cutoff = new Date();
      
      switch (filters.period) {
        case 'week':
          cutoff.setDate(now.getDate() - 7);
          break;
        case 'month':
          cutoff.setMonth(now.getMonth() - 1);
          break;
        case 'year':
          cutoff.setFullYear(now.getFullYear() - 1);
          break;
      }
      
      filtered = filtered.filter(w => new Date(w.date) >= cutoff);
    }

    if (showPRsOnly) {
      filtered = filtered.filter(w => w.isPersonalRecord);
    }

    setFilteredWorkouts(filtered);
  };

  const handleFilter = (type, value) => {
    setFilters({ ...filters, [type]: value });
    if (type === 'exercise' && value) {
      setSelectedExercise(value);
      fetchStats(value);
    } else {
      setStats(null);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      const res = await axios.post(`${API_URL}/workouts`, {
        name: form.name,
        reps: parseInt(form.reps),
        weight: parseFloat(form.weight)
      });

      if (res.data.success) {
        setForm({ name: '', reps: '', weight: '' });
        fetchWorkouts();
        fetchExercises();
        if (res.data.data.isPersonalRecord) {
          alert('🎉 NIEUWE PR!');
        }
      }
    } catch (err) {
      setError(err.response?.data?.errors?.join(', ') || 'Fout bij toevoegen');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (!confirm('Verwijderen?')) return;
    try {
      await axios.delete(`${API_URL}/workouts/${id}`);
      fetchWorkouts();
      fetchExercises();
    } catch (err) {
      setError('Kon niet verwijderen');
    }
  };

  const exportToCSV = () => {
    const headers = ['Exercise,Reps,Weight,Date,PR'];
    const rows = filteredWorkouts.map(w => 
      `${w.name},${w.reps},${w.weight},${new Date(w.date).toLocaleDateString()},${w.isPersonalRecord ? 'Yes' : 'No'}`
    );
    const csv = [headers, ...rows].join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `workouts-${Date.now()}.csv`;
    a.click();
  };

  return (
    <div className="app">
      <h1>💪 Workout Tracker</h1>

      <div className="container">
        <div className="card">
          <h2>Nieuwe Workout</h2>
          <form onSubmit={handleSubmit}>
            <input
              type="text"
              placeholder="Exercise (bijv. Bench Press)"
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              required
            />
            <input
              type="number"
              placeholder="Reps"
              value={form.reps}
              onChange={(e) => setForm({ ...form, reps: e.target.value })}
              required
            />
            <input
              type="number"
              step="0.1"
              placeholder="Weight (kg)"
              value={form.weight}
              onChange={(e) => setForm({ ...form, weight: e.target.value })}
              required
            />
            <button type="submit" disabled={loading}>
              {loading ? 'Toevoegen...' : 'Toevoegen'}
            </button>
          </form>
          {error && <p className="error">{error}</p>}
        </div>

        <div className="card">
          <h2>Filters</h2>
          <FilterPanel
            exercises={exercises}
            onFilter={handleFilter}
            showPRsOnly={showPRsOnly}
            setShowPRsOnly={setShowPRsOnly}
          />
          
          {stats && stats.totalSets > 0 && (
            <div className="stats">
              <h3>📊 Stats: {stats.exerciseName}</h3>
              <p><strong>Total Volume:</strong> {stats.totalVolume.toFixed(0)} kg</p>
              <p><strong>Max Weight:</strong> {stats.maxWeight} kg</p>
              <p><strong>Gemiddeld:</strong> {stats.averageWeight.toFixed(1)} kg</p>
              <p><strong>Total Sets:</strong> {stats.totalSets}</p>
            </div>
          )}
        </div>
      </div>

      {selectedExercise && (
        <div className="card">
          <h2>Progress Chart</h2>
          <ProgressChart workouts={workouts} exerciseName={selectedExercise} />
        </div>
      )}

      <div className="card">
        <div className="workout-header">
          <h2>Workouts ({filteredWorkouts.length})</h2>
          <button onClick={exportToCSV} className="export-btn">
            📥 Export CSV
          </button>
        </div>
        
        <div className="workout-list">
          {filteredWorkouts.length === 0 ? (
            <p>Geen workouts gevonden met deze filters</p>
          ) : (
            filteredWorkouts.map(w => (
              <div key={w.id} className={`workout-item ${w.isPersonalRecord ? 'pr' : ''}`}>
                <div className="workout-info">
                  <strong>{w.name}</strong>
                  <span>{w.reps} reps × {w.weight} kg</span>
                  {w.isPersonalRecord && <span className="badge">🏆 PR</span>}
                  <small>{new Date(w.date).toLocaleDateString('nl-NL')}</small>
                </div>
                <button 
                  className="delete-btn" 
                  onClick={() => handleDelete(w.id)}
                >
                  ×
                </button>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
}

export default App;