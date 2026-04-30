import axios from "@/axios"

export async function getRecentHeartrate(smartwatchId: string, token: string) {
    const oneHourAgo = new Date(Date.now() - 3600 * 1000)

    const res = await axios.get(
        import.meta.env.VITE_API_URL + `/Health/HeartRate`,
        {
            params: {
                smartwatchId,
                startTime: oneHourAgo.toISOString(),
                endTime: new Date().toISOString()
            },
            headers: {
                Authorization: `Bearer ${token}`,
            },
        }
    )

    return res.data
}

export async function getStepsToday(smartwatchId: string, token: string): Promise<number> {
    const todayMidnight = new Date()
    todayMidnight.setHours(0,0,0,0)

    const res = await axios.get(
        import.meta.env.VITE_API_URL + `/Health/Steps`,
        {
            params: {
                smartwatchId,
                startTime: todayMidnight.toISOString(),
                endTime: new Date().toISOString()
            },
            headers: {
                Authorization: `Bearer ${token}`,
            },
        }
    )

    return res.data.steps
}

export async function getActiveTime(smartwatchId: string, token: string): Promise<number> {
    const todayMidnight = new Date()
    todayMidnight.setHours(0,0,0,0)

    const res = await axios.get(
        import.meta.env.VITE_API_URL + `/Health/ActiveTime`,
        {
            params: {
                smartwatchId,
                startTime: todayMidnight.toISOString(),
                endTime: new Date().toISOString()
            },
            headers: {
                Authorization: `Bearer ${token}`,
            },
        }
    )

    return res.data
}