import { render, screen, fireEvent, waitFor, act } from '@testing-library/react'
import axios from 'axios'
import App from './App'

jest.mock('axios')

const mockItems = [
  { id: '1', description: 'Item 1', isCompleted: false },
  { id: '2', description: 'Item 2', isCompleted: false }
]

beforeEach(() => {
  axios.get.mockResolvedValue({ data: mockItems })
})

afterEach(() => {
  jest.clearAllMocks()
})

test('Renders the footer text', () => {
  render(<App />)
  const footerElement = screen.getByText(/clearpoint.digital/i)
  expect(footerElement).toBeInTheDocument()
})

test('Renders initial todo items', async () => {
  await act(async () => {
    render(<App />)
  })
  
  const itemElements = await screen.findAllByRole('row')
  expect(itemElements).toHaveLength(mockItems.length + 1) // Including header row
})

test('Handles API errors gracefully', async () => {
  axios.get.mockRejectedValueOnce(new Error('Error fetching items'))

  await act(async () => {
    render(<App />)
  })

  await waitFor(() => {
    const errorElement = screen.getByText(/Error fetching items/i)
    expect(errorElement).toBeInTheDocument()
  })
})
