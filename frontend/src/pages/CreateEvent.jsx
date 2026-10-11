import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { createEvent } from '../api.js';

export default function CreateEvent() {
  const navigate = useNavigate();
  const [form, setForm] = useState({ title: '', description: '', location: '', startsAt: '', capacity: 10 });
  const [error, setError] = useState('');

  const set = (k) => (e) => setForm({ ...form, [k]: e.target.value });

  const submit = async (e) => {
    e.preventDefault();
    setError('');

    try {
      const created = await createEvent({
        ...form,
        capacity: Number(form.capacity),
        startsAt: new Date(form.startsAt).toISOString(),
      });
      
      navigate(`/events/${created.id}`);
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <>
      <h1>Create event</h1>
      <form className="stack" onSubmit={submit}>
        <label>Title<input required value={form.title} onChange={set('title')} /></label>
        <label>Description<textarea value={form.description} onChange={set('description')} /></label>
        <label>Location<input value={form.location} onChange={set('location')} /></label>
        <label>Starts at<input required type="datetime-local" value={form.startsAt} onChange={set('startsAt')} /></label>
        <label>Capacity<input required type="number" min="1" value={form.capacity} onChange={set('capacity')} /></label>
        <button>Create</button>
      </form>
      {error && <p className="error">{error}</p>}
    </>
  );
}
