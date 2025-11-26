import { Line } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

function ProgressChart({ workouts, exerciseName }) {
  const filtered = workouts
    .filter(w => w.name === exerciseName)
    .sort((a, b) => new Date(a.date) - new Date(b.date));

  if (filtered.length === 0) {
    return <p>Nog geen data voor deze exercise</p>;
  }

  const data = {
    labels: filtered.map(w => new Date(w.date).toLocaleDateString('nl-NL')),
    datasets: [
      {
        label: 'Weight (kg)',
        data: filtered.map(w => w.weight),
        borderColor: '#667eea',
        backgroundColor: 'rgba(102, 126, 234, 0.1)',
        tension: 0.3,
        fill: true
      }
    ]
  };

  const options = {
    responsive: true,
    plugins: {
      legend: {
        display: false
      },
      title: {
        display: true,
        text: `${exerciseName} - Progress`,
        font: { size: 16 }
      }
    },
    scales: {
      y: {
        beginAtZero: false,
        title: {
          display: true,
          text: 'Weight (kg)'
        }
      }
    }
  };

  return <Line data={data} options={options} />;
}

export default ProgressChart;