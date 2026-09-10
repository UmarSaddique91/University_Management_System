import React from 'react'
import Layout from '../layout/Layout'
import StatCard from '../components/StatCard';
import EnrollmentTrend from '../components/EnrollmentTrend';
import { UserRound,Landmark,LibraryBig,NotebookTabs,UsersRound} from 'lucide-react'

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


        {/* StatBoxes  */}
        
        <div className='flex justify-around grid grid-cols-5 gap-4 mt-3'>
          <StatCard

        Icon= {UserRound}
        Title="Total Studetns"
        Value="1,248"
        Change="↑ 12%"
        SubText="+134 this month"
        iconBg=" bg-blue-500/30"
        cardBg="bg-blue-500/5"
        iconcolor="text-blue-700"
        />

        <StatCard

        Icon= {UsersRound}
        Title="Total Teachers"
        Value="1,248"
        Change="↑ 12%"
        SubText="+134 this month"
        iconBg=" bg-green-700/30"
        cardBg="bg-green-700/5"
        iconcolor="text-green-700"
        />

        <StatCard

        Icon= {NotebookTabs}
        Title="Total Courses"
        Value="1,248"
        Change="↑ 12%"
        SubText="+134 this month"
        iconBg=" bg-purple-700/30"
        cardBg="bg-purple-700/5"
        iconcolor="text-purple-700"
        />

        <StatCard

        Icon= {Landmark}
        Title="Total Departments"
        Value="1,248"
        Change="↑ 12%"
        SubText="+134 this month"
        iconBg=" bg-yellow-700/30"
        cardBg="bg-yellow-700/5"
        iconcolor="text-yellow-700"
        
        />

        <StatCard

        Icon= {LibraryBig}
        Title="Total Enrollments"
        Value="1,248"
        Change="↑ 12%"
        SubText="+134 this month"
        iconBg=" bg-red-700/30"
        cardBg="bg-red-700/5"
        iconcolor="text-red-700"
        />
      </div>

        {/* Student Enrollment Trend Section */}
        <div className='mt-4 w-full lg:w-[62%]'>
          <EnrollmentTrend />
        </div>
        </div>
    </Layout>
 
  )
}

export default DashBoard