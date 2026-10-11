import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { getAvailability, getEvent, reserve } from '../api.js';

export default function EventDetail({ userId }) {
  const { id } = useParams();
  const [event, setEvent] = useState(null);
  const [availability, setAvailability] = useState(null);
  const [seats, setSeats] = useState(1);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');

  const load = useCallback(async () => {
    try {
      const [e, a] = await Promise.all([getEvent(id), getAvailability(id)]);
      setEvent(e);
      setAvailability(a);
    } catch (err) {
      setError(err.message);
    }
  }, [id]);

  useEffect(() => { load(); }, [load]);

  const submit = async (ev) => {
    ev.preventDefault();
    setError('');
    setMessage('');
    
    try {
      await reserve(id, userId, Number(seats));
      setMessage('Seats reserved!');
      load();
    } catch (err) {
      setError(err.message);
    }
  };

  if (!event) return error ? <p className="error">{error}</p> : <p>Loading...</p>;

  return (
    <>
      <h1>{event.title}</h1>
      <p>{event.description}</p>
      <p>
        {new Date(event.startsAt).toLocaleString()} · {event.location || 'TBA'}
      </p>
      {availability && (
        <strong>{availability.remaining}</strong>
      )}
      <form onSubmit={submit}>
        <label>
          Seats
          <input type="number" min="1" value={seats} onChange={(e) => setSeats(e.target.value)} />
        </label>
        <button disabled={!userId}>Reserve</button>
        {!userId && <small>Enter a user id at the top first.</small>}
      </form>
      {message && <p className="ok">{message}</p>}
      {error && <p className="error">{error}</p>}
    </>
  );
}
