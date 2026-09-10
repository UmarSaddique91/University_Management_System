import React from 'react'
import Layout from '../layout/Layout'

const DashBoard = () => {
const currentDate= new Date().toLocaleDateString("en-GB",{

  weekday:"long",
  day:"2-digit",
  month:"long",
  year:"numeric"
});


  return (

    <Layout>
      <div>

        {/* DashboardIntro  */}
        <div className='flex justify-between items-center text-black'>
          <div>
            <h1 className='text-[18px] font-bold'>Dashboard</h1>
          <span className='text-[14px] text-slate-600'>Welcome back, Admin! Here's what's hapenning at your university. </span>
          </div>
          
          <div className='text-[14px] font-semibold'>
            {currentDate}
          </div>
          
          
        </div>
      </div>
    </Layout>
 
  )
}

export default DashBoard