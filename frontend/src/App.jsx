import { useState, useEffect } from 'react';
import axios from 'axios';
import './App.css';

const API_URL = 'http://localhost:5274/api';

function App() {
  const [workouts, setWorkouts] = useState([]);
  const [exercises, setExercises] = useState([]);
  const [stats, setStats] = useState(null);
  const [selectedExercise, setSelectedExercise] = useState('');
  const [form, setForm] = useState({ name: '', reps: '', weight: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    fetchWorkouts();
    fetchExercises();
  }, []);

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

  const handleExerciseSelect = (name) => {
    setSelectedExercise(name);
    fetchStats(name);
  };

  return (
    <div className="app">
      <h1>💪 Workout Tracker</h1>

      <div className="container">
        {/* Formulier */}
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

        {/* Exercise Filter */}
        <div className="card">
          <h2>Filter op Exercise</h2>
          <select 
            value={selectedExercise} 
            onChange={(e) => handleExerciseSelect(e.target.value)}
          >
            <option value="">-- Alle exercises --</option>
            {exercises.map(ex => (
              <option key={ex} value={ex}>{ex}</option>
            ))}
          </select>

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

      {/* Workout Lijst */}
      <div className="card">
        <h2>Workouts ({workouts.length})</h2>
        <div className="workout-list">
          {workouts.length === 0 ? (
            <p>Nog geen workouts. Voeg er een toe!</p>
          ) : (
            workouts.map(w => (
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